// The Entity class is rather simple. It contains a few basic entity properties
// like health, mana and level that all inheriting classes like Players and
// Monsters can use.
//
// Entities also have a _target_ Entity that can't be synchronized with a
// SyncVar. Instead we created a EntityTargetSync component that takes care of
// that for us.
//
// Entities use a deterministic finite state machine to handle IDLE/MOVING/DEAD/
// CASTING etc. states and events. Using a deterministic FSM means that we react
// to every single event that can happen in every state (as opposed to just
// taking care of the ones that we care about right now). This means a bit more
// code, but it also means that we avoid all kinds of weird situations like 'the
// monster doesn't react to a dead target when casting' etc.
// The next state is always set with the return value of the UpdateServer
// function. It can never be set outside of it, to make sure that all events are
// truly handled in the state machine and not outside of it. Otherwise we may be
// tempted to set a state in CmdBeingTrading etc., but would likely forget of
// special things to do depending on the current state.
//
// Entities also need a kinematic Rigidbody so that OnTrigger functions can be
// called. Note that there is currently a Unity bug that slows down the agent
// when having lots of FPS(300+) if the Rigidbody's Interpolate option is
// enabled. So for now it's important to disable Interpolation - which is a good
// idea in general to increase performance.
using Mirror;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using E.Utility;
using E.Tool;

public enum DamageType : byte { Normal, Block, Crit };

// note: no animator required, towers, dummies etc. may not have one
[RequireComponent(typeof(Rigidbody2D))] // kinematic, only needed for OnTrigger
[RequireComponent(typeof(NetworkProximityGridChecker))]
[RequireComponent(typeof(NavMeshAgent2D))]
[RequireComponent(typeof(AudioSource))]
public abstract partial class Entity : NetworkBehaviour
{
    [Header("【组件】")]
    public NavMeshAgent2D agent;
    public NetworkProximityGridChecker proxchecker;
    public Animator animator;
    new public Collider2D collider;
    public AudioSource audioSource;

    [Header("【角色状态】")]
    [SyncVar, SerializeField, ReadOnly] string state = "IDLE";
    public string State => state;
    [ReadOnly] public Vector2 lookDirection = Vector2.down; // down by default
    [SyncVar, ReadOnly] public double lastCombatTime;
    [ReadOnly] public bool inSafeZone;

    [Header("【当前目标】")]
    [SyncVar] GameObject target;
    public Entity Target
    {
        get { return target != null ? target.GetComponent<Entity>() : null; }
        set { target = value != null ? value.gameObject : null; }
    }

    [Header("【等级】")]
    public int maxLevel = 1;
    [SyncVar] public int level = 1;

    [Header("【生机】")]
    [Tooltip("是否无敌")] public bool invincible = false;
    [SerializeField] protected LinearInt healthMax = new LinearInt { baseValue = 10 };
    public virtual int HealthMax
    {
        get
        {
            // base + passives + buffs
            int passiveBonus = (from skill in skills
                                where skill.level > 0 && skill.data is PassiveSkill
                                select ((PassiveSkill)skill.data).bonusHealthMax.Get(skill.level)).Sum();
            int buffBonus = buffs.Sum(buff => buff.bonusHealthMax);
            return healthMax.Get(level) + passiveBonus + buffBonus;
        }
    }
    [SyncVar] float health = 1;
    public float Health
    {
        get { return Mathf.Min(health, HealthMax); } // min in case hp>hpmax after buff ends etc.
        set { health = Mathf.Clamp(value, 0, HealthMax); }
    }
    [Tooltip("是否恢复生机")] public bool healthRecovery = true;
    [SerializeField] protected LinearFloat healthRecoveryRate = new LinearFloat { baseValue = 0.00001f };
    public virtual float HealthRecoveryRate
    {
        get
        {
            // base + passives + buffs
            float passivePercent = (from skill in skills
                                    where skill.level > 0 && skill.data is PassiveSkill
                                    select ((PassiveSkill)skill.data).bonusHealthPercentPerSecond.Get(skill.level)).Sum();
            float buffPercent = buffs.Sum(buff => buff.bonusHealthPercentPerSecond);
            return healthRecoveryRate.Get(level) * Health * (1 + passivePercent + buffPercent);
        }
    }

    [Header("【脑力】")]
    [SerializeField] protected LinearInt mindMax = new LinearInt { baseValue = 10 };
    public virtual int MindMax
    {
        get
        {
            // base + passives + buffs
            int passiveBonus = (from skill in skills
                                where skill.level > 0 && skill.data is PassiveSkill
                                select ((PassiveSkill)skill.data).bonusMindMax.Get(skill.level)).Sum();
            int buffBonus = buffs.Sum(buff => buff.bonusMindMax);
            return mindMax.Get(level) + passiveBonus + buffBonus;
        }
    }
    [SyncVar] float mind = 1;
    public float Mind
    {
        get { return Mathf.Min(mind, MindMax); } // min in case hp>hpmax after buff ends etc.
        set { mind = Mathf.Clamp(value, 0, MindMax); }
    }
    [Tooltip("是否恢复脑力")] public bool mindRecovery = true;
    [SerializeField] protected LinearFloat mindRecoveryRate = new LinearFloat { baseValue = 0.001f };
    public float MindRecoveryRate
    {
        get
        {
            // base + passives + buffs
            float passivePercent = (from skill in skills
                                    where skill.level > 0 && skill.data is PassiveSkill
                                    select ((PassiveSkill)skill.data).bonusMindPercentPerSecond.Get(skill.level)).Sum();
            float buffPercent = buffs.Sum(buff => buff.bonusMindPercentPerSecond);
            return mindRecoveryRate.Get(level)* Health * (1 + passivePercent + buffPercent);
        }
    }

    [Header("【体力】")]
    [SerializeField] protected LinearInt powerMax = new LinearInt { baseValue = 10 };
    public virtual int PowerMax
    {
        get
        {
            int passiveBonus = (from skill in skills
                                where skill.level > 0 && skill.data is PassiveSkill
                                select ((PassiveSkill)skill.data).bonusPowerMax.Get(skill.level)).Sum();
            int buffBonus = buffs.Sum(buff => buff.bonusPowerMax);
            return powerMax.Get(level) + passiveBonus + buffBonus;
        }
    }
    [SyncVar] float power = 1;
    public float Power
    {
        get { return Mathf.Min(power, PowerMax); } // min in case hp>hpmax after buff ends etc.
        set { power = Mathf.Clamp(value, 0, PowerMax); }
    }
    [Tooltip("是否恢复体力")] public bool powerRecovery = true;
    [SerializeField] protected LinearFloat powerRecoveryRate = new LinearFloat { baseValue = 0.01f };
    public float PowerRecoveryRate
    {
        get
        {
            float passivePercent = (from skill in skills
                                    where skill.level > 0 && skill.data is PassiveSkill
                                    select ((PassiveSkill)skill.data).bonusPowerPercentPerSecond.Get(skill.level)).Sum();
            float buffPercent = buffs.Sum(buff => buff.bonusPowerPercentPerSecond);
            return powerRecoveryRate.Get(level) * Health * (1 + passivePercent + buffPercent);
        }
    }

    [Header("【力量】")]
    [SerializeField] protected LinearInt strength = new LinearInt { baseValue = 1 };
    public virtual int Strength
    {
        get
        {
            // base + passives + buffs
            int passiveBonus = (from skill in skills
                                where skill.level > 0 && skill.data is PassiveSkill
                                select ((PassiveSkill)skill.data).bonusStrength.Get(skill.level)).Sum();
            int buffBonus = buffs.Sum(buff => buff.bonusDamage);
            return strength.Get(level) + passiveBonus + buffBonus;
        }
    }

    [Header("【防御】")]
    [SerializeField] protected LinearInt defense = new LinearInt { baseValue = 1 };
    public virtual int Defense
    {
        get
        {
            // base + passives + buffs
            int passiveBonus = (from skill in skills
                                where skill.level > 0 && skill.data is PassiveSkill
                                select ((PassiveSkill)skill.data).bonusDefense.Get(skill.level)).Sum();
            int buffBonus = buffs.Sum(buff => buff.bonusDefense);
            return defense.Get(level) + passiveBonus + buffBonus;
        }
    }

    [Header("【速度】")]
    [SerializeField] protected LinearInt speed = new LinearInt { baseValue = 2 };
    public virtual int Speed
    {
        get
        {
            // base + passives + buffs
            float passiveBonus = (from skill in skills
                                  where skill.level > 0 && skill.data is PassiveSkill
                                  select ((PassiveSkill)skill.data).bonusSpeed.Get(skill.level)).Sum();
            float buffBonus = buffs.Sum(buff => buff.bonusSpeed);
            return speed.Get(level) + (int)(passiveBonus + buffBonus);
        }
    }
    [SerializeField] protected LinearInt runSpeedMultiple = new LinearInt { baseValue = 3 };
    public virtual int RunSpeedMultiple
    {
        get
        {
            return runSpeedMultiple.Get(level);
        }
    }
    [SerializeField, ReadOnly] protected int currentSpeed = 0;

    [Header("【智慧】")]
    [SerializeField] protected LinearInt intelligence = new LinearInt { baseValue = 1 };
    public virtual int Intelligence
    {
        get
        {
            int passiveBonus = (from skill in skills
                                where skill.level > 0 && skill.data is PassiveSkill
                                select ((PassiveSkill)skill.data).bonusIntelligence.Get(skill.level)).Sum();
            int buffBonus = buffs.Sum(buff => buff.bonusIntelligence);
            return intelligence.Get(level) + passiveBonus + buffBonus;
        }
    }

    [Header("【致晕率】")]
    [SerializeField] protected LinearFloat blockChance;
    public virtual float BlockChance
    {
        get
        {
            // base + passives + buffs
            float passiveBonus = (from skill in skills
                                  where skill.level > 0 && skill.data is PassiveSkill
                                  select ((PassiveSkill)skill.data).bonusBlockChance.Get(skill.level)).Sum();
            float buffBonus = buffs.Sum(buff => buff.bonusBlockChance);
            return blockChance.Get(level) + passiveBonus + buffBonus;
        }
    }

    [Header("【暴击率】")]
    [SerializeField] protected LinearFloat criticalChance;
    public virtual float CriticalChance
    {
        get
        {
            // base + passives + buffs
            float passiveBonus = (from skill in skills
                                  where skill.level > 0 && skill.data is PassiveSkill
                                  select ((PassiveSkill)skill.data).bonusCriticalChance.Get(skill.level)).Sum();
            float buffBonus = buffs.Sum(buff => buff.bonusCriticalChance);
            return criticalChance.Get(level) + passiveBonus + buffBonus;
        }
    }

    [Header("【背包】")]
    public SyncListItemSlot inventory = new SyncListItemSlot();

    [Header("【装备】")]
    public SyncListItemSlot equipment = new SyncListItemSlot();

    [Header("【金钱】")]
    [SyncVar, SerializeField] long money = 0;
    public long Money { get { return money; } set { money = Math.Max(value, 0); } }

    [Header("【受伤特效】")]
    public GameObject damagePopupPrefab;

    [Header("【技能 & 增益】")]
    public ScriptableSkill[] skillTemplates;
    public SyncListSkill skills = new SyncListSkill();
    public SyncListBuff buffs = new SyncListBuff();
    [SyncVar, HideInInspector] public int currentSkill = -1;

    [Header("【技能特效生成点】")]
    [SerializeField] Transform effectMount;
    public virtual Transform EffectMount { get { return effectMount; } }

    [Header("【名称】")]
    public GameObject panName;

    [Header("【公会】")]
    public GameObject panGuild;
    public string guildPrefix = "[";
    public string guildSuffix = "]";

    [Header("【眩晕】")]
    public GameObject panStunned;
    protected double stunTimeEnd;

    [Header("【对话】")]
    public GameObject panDialog;
    [TextArea(1, 30)] public string dialogContent = "";

    [Header("【任务标记】")]
    public GameObject panQuestMark;

    [Header("【颜色】")]
    public Color clrDefault = Color.white;
    public Color clrOffender = Color.magenta;
    public Color clrMurderer = Color.red;
    public Color clrParty = new Color(0.341f, 0.965f, 0.702f);


    private void OnEnable()
    {
        if (panName != null)
        {
            //设置角色名字显示模式
            switch (UIManager.Singleton.entityInfoDisplayMode)
            {
                case UIManager.EntityInfoDisplayMode.AlwaysShow:
                    panName.gameObject.SetActive(true);
                    break;
                case UIManager.EntityInfoDisplayMode.HoverShowOnly:
                    panName.gameObject.SetActive(false);
                    break;
                case UIManager.EntityInfoDisplayMode.HitShowOnly:
                    panName.gameObject.SetActive(false);
                    break;
                case UIManager.EntityInfoDisplayMode.HoverShowAndHitShow:
                    panName.gameObject.SetActive(false);
                    break;
                case UIManager.EntityInfoDisplayMode.AlwaysHide:
                    panName.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }
    }
    private void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.8f);
        if (panName != null)
        {
            if (UIManager.Singleton.entityInfoDisplayMode == UIManager.EntityInfoDisplayMode.HoverShowOnly ||
                UIManager.Singleton.entityInfoDisplayMode == UIManager.EntityInfoDisplayMode.HoverShowAndHitShow)
            {
                panName.gameObject.SetActive(true);
            }
        }
    }
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(1))
        {
            UIManager.Singleton.uiCharacterInfo.SetCharacterEntity(this);
        }
    }
    private void OnMouseDown()
    {
        GetComponent<SpriteRenderer>().color = new Color(0.6f, 0.6f, 0.6f);
    }
    private void OnMouseDrag()
    {

    }
    private void OnMouseUp()
    {
        GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.8f);
    }
    private void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
        if (panName != null)
        {
            if (UIManager.Singleton.entityInfoDisplayMode == UIManager.EntityInfoDisplayMode.HoverShowOnly ||
                UIManager.Singleton.entityInfoDisplayMode == UIManager.EntityInfoDisplayMode.HoverShowAndHitShow)
            {
                panName.gameObject.SetActive(false);
            }
        }
    }
    // networkbehaviour ////////////////////////////////////////////////////////
    protected virtual void Awake()
    {
        // addon system hooks
        Utils.InvokeMany(typeof(Entity), this, "Awake_");
    }

    public override void OnStartServer()
    {
        // health recovery every second
        InvokeRepeating(nameof(Recover), 1, 1);

        // dead if spawned without health
        if (Health == 0) state = "DEAD";

        // addon system hooks
        Utils.InvokeMany(typeof(Entity), this, "OnStartServer_");
    }

    protected virtual void Start()
    {
        // disable animator on server. this is a huge performance boost and
        // definitely worth one line of code (1000 monsters: 22 fps => 32 fps)
        // (!isClient because we don't want to do it in host mode either)
        // (OnStartServer doesn't know isClient yet, Start is the only option)
        if (!isClient) animator.enabled = false;
    }

    // monsters, npcs etc. don't have to be updated if no player is around
    // checking observers is enough, because lonely players have at least
    // themselves as observers, so players will always be updated
    // and dead monsters will respawn immediately in the first update call
    // even if we didn't update them in a long time (because of the 'end'
    // times)
    // -> update only if:
    //    - observers are null (they are null in clients)
    //    - if they are not null, then only if at least one (on server)
    //    - if the entity is hidden, otherwise it would never be updated again
    //      because it would never get new observers
    // -> can be overwritten if necessary (e.g. pets might be too far from
    //    observers but should still be updated to run to owner)
    public virtual bool IsWorthUpdating()
    {
        return netIdentity.observers == null ||
               netIdentity.observers.Count > 0 ||
               IsHidden();
    }

    // entity logic will be implemented with a finite state machine
    // -> we should react to every state and to every event for correctness
    // -> we keep it functional for simplicity
    // note: can still use LateUpdate for Updates that should happen in any case
    [Obsolete]
    void Update()
    {
        // only update if it's worth updating (see IsWorthUpdating comments)
        // -> we also clear the target if it's hidden, so that players don't
        //    keep hidden (respawning) monsters as target, hence don't show them
        //    as target again when they are shown again
        if (IsWorthUpdating())
        {
            // always apply speed to agent
            agent.speed = Speed;

            if (isClient)
            {
                UpdateClient();
            }
            if (isServer)
            {
                CleanupBuffs();
                if (Target != null && Target.IsHidden()) Target = null;
                state = UpdateServer();
            }

            // update look direction on server and client (saves a SyncVar)
            // -> always look at move or target direction (if any), otherwise
            //    use the last one when IDLE
            // -> always orthonormal like (0,1) etc. and never (0, 0.5) so the blend
            //    tree doesn't actually blend between sprite animations
            // -> with default value so that default is played instead of nothing for
            //    Vector2.zero cases
            if (agent.velocity != Vector2.zero)
                lookDirection = Utils.OrthonormalVector2(agent.velocity, lookDirection);
            else if (Target != null)
                lookDirection = Utils.OrthonormalVector2(Target.transform.position - transform.position, lookDirection);

            // addon system hooks
            Utils.InvokeMany(typeof(Entity), this, "Update_");
        }

        // update overlays in any case, except on server-only mode
        // (also update for character selection previews etc. then)
        if (!isServerOnly) UpdateOverlays();
    }

    // update for server. should return the new state.
    protected abstract string UpdateServer();

    // update for client.
    [Obsolete]
    protected abstract void UpdateClient();

    // can be overwritten for more overlays
    protected virtual void UpdateOverlays()
    {
        if (panName != null)
        {
            panName.GetComponentInChildren<Text>().text = name;
        }
        if (dialogContent != null)
        {
            panDialog.GetComponentInChildren<Text>().text = dialogContent;
            if (dialogContent != null && dialogContent != "")
            {
                panDialog.SetActive(true);
            }
            else
            {
                panDialog.SetActive(false);
            }
        }
        if (panGuild != null)
        {
            panGuild.SetActive(false);
        }
        if (panStunned != null)
        {
            panStunned.SetActive(State == "STUNNED");
        }
        if (panQuestMark != null)
        {
            panQuestMark.SetActive(false);
        }
    }

    // visibility //////////////////////////////////////////////////////////////
    // hide a entity
    // note: using SetActive won't work because its not synced and it would
    //       cause inactive objects to not receive any info anymore
    // note: this won't be visible on the server as it always sees everything.
    [Server]
    public void Hide()
    {
        proxchecker.forceHidden = true;
    }

    [Server]
    public void Show()
    {
        proxchecker.forceHidden = false;
    }

    // is the entity currently hidden?
    // note: usually the server is the only one who uses forceHidden, the
    //       client usually doesn't know about it and simply doesn't see the
    //       GameObject.
    public bool IsHidden() => proxchecker.forceHidden;

    public float VisRange() => NetworkProximityGridChecker.visRange;

    // -> agent.hasPath will be true if stopping distance > 0, so we can't
    //    really rely on that.
    // -> pathPending is true while calculating the path, which is good
    // -> remainingDistance is the distance to the last path point, so it
    //    also works when clicking somewhere onto a obstacle that isn't
    //    directly reachable.
    // -> velocity is the best way to detect WASD movement
    public bool IsMoving() =>
        agent.pathPending ||
        agent.remainingDistance > agent.stoppingDistance ||
        agent.velocity != Vector2.zero;

    // health & mana ///////////////////////////////////////////////////////////
    public float HealthPercent()
    {
        return (Health != 0 && HealthMax != 0) ? (float)Health / (float)HealthMax : 0;
    }

    [Server]
    public void Revive(float healthPercentage = 1)
    {
        Health = Mathf.RoundToInt(HealthMax * healthPercentage);
    }

    public float MindPercent()
    {
        return (Mind != 0 && MindMax != 0) ? Mind / MindMax : 0;
    }

    /// <summary>
    /// 计算对另一个实体的伤害
    /// </summary>
    /// <param name="entity">目标</param>
    /// <param name="amount">输出伤害</param>
    /// <param name="stunChance">眩晕概率</param>
    /// <param name="stunTime">眩晕时间</param>
    [Server]
    public virtual void DealDamageAt(Entity entity, int amount, float stunChance = 0, float stunTime = 0)
    {
        int damageDealt = 0;
        DamageType damageType = DamageType.Normal;

        // don't deal any damage if entity is invincible
        if (!entity.invincible)
        {
            // block? (we use < not <= so that block rate 0 never blocks)
            if (UnityEngine.Random.value < entity.BlockChance)
            {
                damageType = DamageType.Block;
            }
            // deal damage
            else
            {
                // subtract defense (but leave at least 1 damage, otherwise
                // it may be frustrating for weaker players)
                damageDealt = Mathf.Max(amount - entity.Defense, 1);

                // critical hit?
                if (UnityEngine.Random.value < CriticalChance)
                {
                    damageDealt *= 2;
                    damageType = DamageType.Crit;
                }

                // deal the damage
                entity.Health -= damageDealt;

                // stun?
                if (UnityEngine.Random.value < stunChance)
                {
                    // dont allow a short stun to overwrite a long stun
                    // => if a player is hit with a 10s stun, immediately
                    //    followed by a 1s stun, we don't want it to end in 1s!
                    double newStunEndTime = NetworkTime.time + stunTime;
                    entity.stunTimeEnd = Math.Max(newStunEndTime, stunTimeEnd);
                }
            }
        }

        // let's make sure to pull aggro in any case so that archers
        // are still attacked if they are outside of the aggro range
        entity.OnAggro(this);

        // show effects on clients
        entity.RpcOnDamageReceived(damageDealt, damageType);

        // reset last combat time for both
        lastCombatTime = NetworkTime.time;
        entity.lastCombatTime = NetworkTime.time;

        // addon system hooks
        Utils.InvokeMany(typeof(Entity), this, "DealDamageAt_", entity, amount);
    }

    /// <summary>
    /// 显示受伤提示
    /// </summary>
    /// <param name="amount">伤害值</param>
    /// <param name="damageType">伤害类型</param>
    [Client]
    void ShowDamagePopup(int amount, DamageType damageType)
    {
        // spawn the damage popup (if any) and set the text
        if (damagePopupPrefab != null)
        {
            // showing it above their head looks best, and we don't have to use
            // a custom shader to draw world space UI in front of the entity
            Bounds bounds = collider.bounds;
            Vector2 position = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);

            GameObject popup = Instantiate(damagePopupPrefab, position, Quaternion.identity);
            if (damageType == DamageType.Normal)
                popup.GetComponentInChildren<TextMesh>().text = amount.ToString();
            else if (damageType == DamageType.Block)
                popup.GetComponentInChildren<TextMesh>().text = "<i>Block!</i>";
            else if (damageType == DamageType.Crit)
                popup.GetComponentInChildren<TextMesh>().text = amount + " Crit!";
        }
    }

    [ClientRpc]
    void RpcOnDamageReceived(int amount, DamageType damageType)
    {
        // show popup above receiver's head in all observers via ClientRpc
        ShowDamagePopup(amount, damageType);

        // addon system hooks
        Utils.InvokeMany(typeof(Entity), this, "OnDamageReceived_", amount, damageType);
    }

    [Server]
    public void Recover()
    {
        if (enabled && Health > 0)
        {
            if (healthRecovery) Health += HealthRecoveryRate;
            if (mindRecovery) Mind += MindRecoveryRate;
            if (powerRecovery) Power += PowerRecoveryRate;
        }
    }

    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by the AggroArea (if any) on clients and server
    public virtual void OnAggro(Entity entity) { }

    // skill system ////////////////////////////////////////////////////////////
    // helper function to find a skill index
    public int GetSkillIndexByName(string skillName)
    {
        return skills.FindIndex(skill => skill.name == skillName);
    }

    // helper function to find a buff index
    public int GetBuffIndexByName(string buffName)
    {
        return buffs.FindIndex(buff => buff.name == buffName);
    }

    // we need a function to check if an entity can attack another.
    // => overwrite to add more cases like 'monsters can only attack players'
    //    or 'player can attack pets but not own pet' etc.
    // => raycast NavMesh to prevent attacks through walls, while allowing
    //    attacks through steep hills etc. (unlike Physics.Raycast). this is
    //    very important to prevent exploits where someone might try to attack a
    //    boss monster through a dungeon wall, etc.
    public virtual bool CanAttack(Entity entity)
    {
        return Health > 0 &&
               entity.Health > 0 &&
               entity != this &&
               !inSafeZone && !entity.inSafeZone &&
               !NavMesh2D.Raycast(transform.position, entity.transform.position, out NavMeshHit2D hit, NavMesh2D.AllAreas); ;
    }

    // the first check validates the caster
    // (the skill won't be ready if we check self while casting it. so the
    //  checkSkillReady variable can be used to ignore that if needed)
    // has a weapon (important for projectiles etc.), no cooldown, hp, mp?
    public bool CastCheckSelf(Skill skill, bool checkSkillReady = true) =>
        skill.CheckSelf(this, checkSkillReady);

    // the second check validates the target and corrects it for the skill if
    // necessary (e.g. when trying to heal an npc, it sets target to self first)
    // (skill shots that don't need a target will just return true if the user
    //  wants to cast them at a valid position)
    public bool CastCheckTarget(Skill skill) =>
        skill.CheckTarget(this);

    // the third check validates the distance between the caster and the target
    // (target entity or target position in case of skill shots)
    // note: castchecktarget already corrected the target (if any), so we don't
    //       have to worry about that anymore here
    public bool CastCheckDistance(Skill skill, out Vector2 destination) =>
        skill.CheckDistance(this, out destination);

    // starts casting
    public void StartCastSkill(Skill skill)
    {
        // start casting and set the casting end time
        skill.castTimeEnd = NetworkTime.time + skill.castTime;

        // save modifications
        skills[currentSkill] = skill;

        // rpc for client sided effects
        RpcSkillCastStarted();
    }

    // finishes casting. casting and waiting has to be done in the state machine
    public void FinishCastSkill(Skill skill)
    {
        // * check if we can currently cast a skill (enough mana etc.)
        // * check if we can cast THAT skill on THAT target
        // note: we don't check the distance again. the skill will be cast even
        //   if the target walked a bit while we casted it (it's simply better
        //   gameplay and less frustrating)
        if (CastCheckSelf(skill, false) && CastCheckTarget(skill))
        {
            // let the skill template handle the action
            skill.Apply(this);

            // rpc for client sided effects
            // -> pass that skill because skillIndex might be reset in the mean
            //    time, we never know
            RpcSkillCastFinished(skill);

            // decrease mana in any case
            Mind -= skill.mindCosts;

            // start the cooldown (and save it in the struct)
            skill.cooldownEnd = NetworkTime.time + skill.cooldown;

            // save any skill modifications in any case
            skills[currentSkill] = skill;
        }
        else
        {
            // not all requirements met. no need to cast the same skill again
            currentSkill = -1;
        }
    }

    // helper function to add or refresh a buff
    public void AddOrRefreshBuff(Buff buff)
    {
        // reset if already in buffs list, otherwise add
        int index = buffs.FindIndex(b => b.name == buff.name);
        if (index != -1) buffs[index] = buff;
        else buffs.Add(buff);
    }

    // helper function to remove all buffs that ended
    void CleanupBuffs()
    {
        for (int i = 0; i < buffs.Count; ++i)
        {
            if (buffs[i].BuffTimeRemaining() == 0)
            {
                buffs.RemoveAt(i);
                --i;
            }
        }
    }

    // skill cast started rpc for client sided effects
    // note: no need to pass skillIndex, currentSkill is synced anyway
    [ClientRpc]
    public void RpcSkillCastStarted()
    {
        // validate: still alive and valid skill?
        if (Health > 0 && 0 <= currentSkill && currentSkill < skills.Count)
        {
            skills[currentSkill].data.OnCastStarted(this);
        }
    }

    // skill cast finished rpc for client sided effects
    // note: no need to pass skillIndex, currentSkill is synced anyway
    [ClientRpc]
    public void RpcSkillCastFinished(Skill skill)
    {
        // validate: still alive?
        if (Health > 0)
        {
            // call scriptableskill event
            skill.data.OnCastFinished(this);

            // maybe some other component needs to know about it too
            SendMessage("OnSkillCastFinished", skill, SendMessageOptions.DontRequireReceiver);
        }
    }

    // inventory ///////////////////////////////////////////////////////////////
    // helper function to find an item in the inventory
    public int GetInventoryIndexByName(string itemName)
    {
        return inventory.FindIndex(slot => slot.amount > 0 && slot.item.Name == itemName);
    }

    // helper function to count the free slots
    public int InventorySlotsFree()
    {
        return inventory.Count(slot => slot.amount == 0);
    }

    // helper function to calculate the total amount of an item type in inventory
    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
    public int InventoryCount(Item item)
    {
        return (from slot in inventory
                where slot.amount > 0 && slot.item.Equals(item)
                select slot.amount).Sum();
    }

    // helper function to remove 'n' items from the inventory
    public bool InventoryRemove(Item item, int amount)
    {
        for (int i = 0; i < inventory.Count; ++i)
        {
            ItemSlot slot = inventory[i];
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            if (slot.amount > 0 && slot.item.Equals(item))
            {
                // take as many as possible
                amount -= slot.DecreaseAmount(amount);
                inventory[i] = slot;

                // are we done?
                if (amount == 0) return true;
            }
        }

        // if we got here, then we didn't remove enough items
        return false;
    }

    // helper function to check if the inventory has space for 'n' items of type
    // -> the easiest solution would be to check for enough free item slots
    // -> it's better to try to add it onto existing stacks of the same type
    //    first though
    // -> it could easily take more than one slot too
    // note: this checks for one item type once. we can't use this function to
    //       check if we can add 10 potions and then 10 potions again (e.g. when
    //       doing player to player trading), because it will be the same result
    public bool InventoryCanAdd(Item item, int amount)
    {
        // go through each slot
        for (int i = 0; i < inventory.Count; ++i)
        {
            // empty? then subtract maxstack
            if (inventory[i].amount == 0)
                amount -= item.MaxStack;
            // not empty. same type too? then subtract free amount (max-amount)
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            else if (inventory[i].item.Equals(item))
                amount -= (inventory[i].item.MaxStack - inventory[i].amount);

            // were we able to fit the whole amount already?
            if (amount <= 0) return true;
        }

        // if we got here than amount was never <= 0
        return false;
    }

    // helper function to put 'n' items of a type into the inventory, while
    // trying to put them onto existing item stacks first
    // -> this is better than always adding items to the first free slot
    // -> function will only add them if there is enough space for all of them
    public bool InventoryAdd(Item item, int amount)
    {
        // we only want to add them if there is enough space for all of them, so
        // let's double check
        if (InventoryCanAdd(item, amount))
        {
            // add to same item stacks first (if any)
            // (otherwise we add to first empty even if there is an existing
            //  stack afterwards)
            for (int i = 0; i < inventory.Count; ++i)
            {
                // not empty and same type? then add free amount (max-amount)
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (inventory[i].amount > 0 && inventory[i].item.Equals(item))
                {
                    ItemSlot temp = inventory[i];
                    amount -= temp.IncreaseAmount(amount);
                    inventory[i] = temp;
                }

                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return true;
            }

            // add to empty slots (if any)
            for (int i = 0; i < inventory.Count; ++i)
            {
                // empty? then fill slot with as many as possible
                if (inventory[i].amount == 0)
                {
                    int add = Mathf.Min(amount, item.MaxStack);
                    inventory[i] = new ItemSlot(item, add);
                    amount -= add;
                }

                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return true;
            }
            // we should have been able to add all of them
            if (amount != 0) Debug.LogError("inventory add failed: " + item.Name + " " + amount);
        }
        return false;
    }

    // equipment ///////////////////////////////////////////////////////////////
    public int GetEquipmentIndexByName(string itemName)
    {
        return equipment.FindIndex(slot => slot.amount > 0 && slot.item.Name == itemName);
    }

    // helper function to find the equipped weapon index
    // -> works for all entity types. returns -1 if no weapon equipped.
    public int GetEquippedWeaponIndex()
    {
        return equipment.FindIndex(slot => slot.amount > 0 &&
                                           slot.item.Data is WeaponItem);
    }

    // get currently equipped weapon category to check if skills can be casted
    // with this weapon. returns "" if none.
    public string GetEquippedWeaponCategory()
    {
        // find the weapon slot
        int index = GetEquippedWeaponIndex();
        return index != -1 ? ((WeaponItem)equipment[index].item.Data).category : "";
    }

    // death ///////////////////////////////////////////////////////////////////
    // universal OnDeath function that takes care of all the Entity stuff.
    // should be called by inheriting classes' finite state machine on death.
    [Server]
    protected virtual void OnDeath()
    {
        // clear movement/buffs/target/cast
        agent.ResetMovement();
        buffs.Clear();
        Target = null;
        currentSkill = -1;

        // addon system hooks
        Utils.InvokeMany(typeof(Entity), this, "OnDeath_");
    }

    // ontrigger ///////////////////////////////////////////////////////////////
    // protected so that inheriting classes can use OnTrigger too, while also
    // calling those here via base.OnTriggerEnter/Exit
    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        // check if trigger first to avoid GetComponent tests for environment
        if (col.isTrigger && col.GetComponent<SafeZone>())
            inSafeZone = true;
    }

    protected virtual void OnTriggerExit2D(Collider2D col)
    {
        // check if trigger first to avoid GetComponent tests for environment
        if (col.isTrigger && col.GetComponent<SafeZone>())
            inSafeZone = false;
    }
}
