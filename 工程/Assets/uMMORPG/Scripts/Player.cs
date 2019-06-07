// All player logic was put into this class. We could also split it into several
// smaller components, but this would result in many GetComponent calls and a
// more complex syntax.
//
// The default Player class takes care of the basic player logic like the state
// machine and some properties like damage and defense.
//
// The Player class stores the maximum experience for each level in a simple
// array. So the maximum experience for level 1 can be found in expMax[0] and
// the maximum experience for level 2 can be found in expMax[1] and so on. The
// player's health and mana are also level dependent in most MMORPGs, hence why
// there are hpMax and mpMax arrays too. We can find out a players's max health
// in level 1 by using hpMax[0] and so on.
//
// The class also takes care of selection handling, which detects 3D world
// clicks and then targets/navigates somewhere/interacts with someone.
//
// Animations are not handled by the NetworkAnimator because it's still very
// buggy and because it can't really react to movement stops fast enough, which
// results in moonwalking. Not synchronizing animations over the network will
// also save us bandwidth
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using E.Utility;
using E.Game;

public enum TradeStatus : byte { Free, Locked, Accepted }
public enum CraftingState : byte { None, InProgress, Success, Failed }

[Serializable]
public partial struct SkillbarEntry
{
    public string reference;
    public KeyCode hotKey;
}

[Serializable]
public partial struct EquipmentInfo
{
    public string requiredCategory;
    public SubAnimation location;
    public ScriptableItemAndAmount defaultItem;
}

[Serializable]
public partial struct ItemMallCategory
{
    public string category;
    public ScriptableItem[] items;
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerChat))]
[RequireComponent(typeof(NetworkName))]
[RequireComponent(typeof(NetworkNavMeshAgentRubberbanding2D))]
public partial class Player : Entity
{
    [Header("【组件】")]
    public PlayerChat chat;
    public Camera avatarCamera;
    public NetworkNavMeshAgentRubberbanding2D rubberbanding;

    [Header("【图标】")]
    public Sprite classIcon; // for character selection
    public Sprite portraitIcon; // for top left portrait

    [Header("【meta 信息】")]
    [ReadOnly] public string account = "";
    [ReadOnly] public string className = "";

    public static Player localPlayer;

    public override int HealthMax
    {
        get
        {
            // calculate equipment bonus
            int equipmentBonus = (from slot in equipment
                                  where slot.amount > 0
                                  select ((EquipmentItem)slot.item.data).healthBonus).Sum();

            // base (health + buff) + equip + attributes
            return base.HealthMax + equipmentBonus + healthAdditional;
        }
    }
    public override int MindMax
    {
        get
        {
            int equipmentBonus = (from slot in equipment
                                  where slot.amount > 0
                                  select ((EquipmentItem)slot.item.data).mindBonus).Sum();

            return base.MindMax + equipmentBonus + mindAdditional;
        }
    }
    public override int PowerMax
    {
        get
        {
            int equipmentBonus = (from slot in equipment
                                  where slot.amount > 0
                                  select ((EquipmentItem)slot.item.data).powerBonus).Sum();

            return base.PowerMax + equipmentBonus + powerAdditional;
        }
    }

    public override int Strength
    {
        get
        {
            int equipmentBonus = (from slot in equipment
                                  where slot.amount > 0
                                  select ((EquipmentItem)slot.item.data).damageBonus).Sum();

            return base.Strength + equipmentBonus + strengthAdditional;
        }
    }
    public override int Defense
    {
        get
        {
            int equipmentBonus = (from slot in equipment
                                  where slot.amount > 0
                                  select ((EquipmentItem)slot.item.data).defenseBonus).Sum();

            return base.Defense + equipmentBonus + defenseAdditional;
        }
    }
    public override int Speed
    {
        get
        {
            // mount speed if mounted, regular speed otherwise
            return ActiveMount != null && ActiveMount.Health > 0 ? ActiveMount.Speed : base.Speed;
        }
    }
    public override int RunSpeedMultiple
    {
        get
        {
            return base.RunSpeedMultiple + runSpeedMultipleAdditional;
        }
    }
    public override int Intelligence
    {
        get
        {
            int equipmentBonus = (from slot in equipment
                                  where slot.amount > 0
                                  select ((EquipmentItem)slot.item.data).intelligenceBonus).Sum();

            return base.Intelligence + equipmentBonus + intelligenceAdditional;
        }
    }

    public override float BlockChance
    {
        get
        {
            // calculate equipment bonus
            float equipmentBonus = (from slot in equipment
                                    where slot.amount > 0
                                    select ((EquipmentItem)slot.item.data).blockChanceBonus).Sum();

            // return base (blockChance + buff) + equip
            return base.BlockChance + equipmentBonus;
        }
    }
    public override float CriticalChance
    {
        get
        {
            // calculate equipment bonus
            float equipmentBonus = (from slot in equipment
                                    where slot.amount > 0
                                    select ((EquipmentItem)slot.item.data).criticalChanceBonus).Sum();

            // return base (criticalChance + buff) + equip
            return base.CriticalChance + equipmentBonus;
        }
    }

    [Header("【附加点数】")]
    [SyncVar] public int healthAdditional = 0;
    [SyncVar] public int mindAdditional = 0;
    [SyncVar] public int powerAdditional = 0;
    [SyncVar] public int strengthAdditional = 0;
    [SyncVar] public int defenseAdditional = 0;
    [SyncVar] public int runSpeedMultipleAdditional = 0;
    [SyncVar] public int intelligenceAdditional = 0;

    [Header("【等级 & 经验】")]
    [SyncVar, SerializeField] long experience = 0;
    public long Experience
    {
        get { return experience; }
        set
        {
            if (value <= experience)
            {
                // decrease
                experience = Math.Max(value, 0);
            }
            else
            {
                // increase with level ups
                // set the new value (which might be more than expMax)
                experience = value;

                // now see if we leveled up (possibly more than once too)
                // (can't level up if already max level)
                while (experience >= ExperienceMax && level < maxLevel)
                {
                    // subtract current level's required exp, then level up
                    experience -= ExperienceMax;
                    ++level;

                    // addon system hooks
                    Utils.InvokeMany(typeof(Player), this, "OnLevelUp_");
                }

                // set to expMax if there is still too much exp remaining
                if (experience > ExperienceMax) experience = ExperienceMax;
            }
        }
    }
    [SerializeField] protected ExponentialLong experienceMax = new ExponentialLong { multiplier = 10, baseValue = 10 };
    public long ExperienceMax { get { return experienceMax.Get(level); } }

    [Header("【技能经验】")]
    [SyncVar] public long skillExperience = 0;

    [Header("【标识物】")]
    public GameObject indicatorPrefab;
    [ReadOnly] public GameObject indicator;

    [Header("【背包】")]
    public int inventorySize = 30;
    public ScriptableItemAndAmount[] defaultItems;
    public KeyCode[] inventorySplitKeys = { KeyCode.LeftShift, KeyCode.RightShift };

    [Header("【废弃物】")]
    [SyncVar] public ItemSlot trash;

    [Header("【装备】")]
    public EquipmentInfo[] equipmentInfo =
    {
        new EquipmentInfo{requiredCategory="Weapon", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Head", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Chest", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Legs", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Shield", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Shoulders", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Hands", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Feet", location=null, defaultItem=new ScriptableItemAndAmount()}
    };

    [Header("【技能条】")]
    public SkillbarEntry[] skillbar =
    {
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha1},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha2},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha3},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha4},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha5},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha6},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha7},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha8},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha9},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha0},
    };

    [Header("【任务】")] // contains active and completed quests (=all)
    public int activeQuestLimit = 10;
    public SyncListQuest quests = new SyncListQuest();

    [Header("【交互】")]
    public float interactionRange = 1;
    public KeyCode targetNearestKey = KeyCode.Tab;
    public bool localPlayerClickThrough = true; // click selection goes through localplayer. feels best.
    public KeyCode cancelActionKey = KeyCode.Escape;
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f;

    [Header("【PvP】")]
    public BuffSkill offenderBuff;
    public BuffSkill murdererBuff;

    [Header("【贸易】")]
    [SyncVar, HideInInspector] public string tradeRequestFrom = "";
    [SyncVar, HideInInspector] public TradeStatus tradeStatus = TradeStatus.Free;
    [SyncVar, HideInInspector] public long tradeOfferGold = 0;
    public SyncListInt tradeOfferItems = new SyncListInt(); // inventory indices

    [Header("【制作合成】")]
    public List<int> craftingIndices = Enumerable.Repeat(-1, ScriptableRecipe.recipeSize).ToList();
    [HideInInspector] public CraftingState craftingState = CraftingState.None; // // client sided
    [SyncVar, HideInInspector] public double craftingTimeEnd; // double for long term precision

    [Header("【商城】")]
    public ItemMallCategory[] itemMallCategories;
    [SyncVar] public long coins = 0;
    public float couponWaitSeconds = 3;

    [Header("【公会】")]
    [SyncVar, HideInInspector] public string guildInviteFrom = "";
    [SyncVar, HideInInspector] public Guild guild; // TODO SyncToOwner later
    public float guildInviteWaitSeconds = 3;

    [Header("【帮派】")]
    [SyncVar, HideInInspector] public Party party; // TODO SyncToOwner later
    [SyncVar, HideInInspector] public string partyInviteFrom = "";
    public float partyInviteWaitSeconds = 3;

    [Header("【宠物】")]
    [SyncVar] GameObject activePet;
    public Pet ActivePet
    {
        get { return activePet != null ? activePet.GetComponent<Pet>() : null; }
        set { activePet = value != null ? value.gameObject : null; }
    }
    public Vector2 PetDestination
    {
        get
        {
            Bounds bounds = collider.bounds;
            return transform.position - transform.right * bounds.size.x;
        }
    }

    [Header("【骑乘物】")]
    [SyncVar] GameObject activeMount;
    public Mount ActiveMount
    {
        get { return activeMount != null ? activeMount.GetComponent<Mount>() : null; }
        set { activeMount = value != null ? value.gameObject : null; }
    }

    [Header("【死亡损失】")]
    public float deathExperienceLossPercent = 0.05f;

    [Header("【下个目标】")]
    [SyncVar] GameObject nextTarget;
    public Entity NextTarget
    {
        get { return nextTarget != null ? nextTarget.GetComponent<Entity>() : null; }
        set { nextTarget = value != null ? value.gameObject : null; }
    }

    [Header("【其它】")]
    // some commands should have delays to avoid DDOS, too much database usage
    // or brute forcing coupons etc. we use one riskyAction timer for all.
    [SyncVar, HideInInspector] public double nextRiskyActionTime = 0; // double for long term precision
    // cache players to save lots of computations
    // (otherwise we'd have to iterate NetworkServer.objects all the time)
    // => on server: all online players
    // => on client: all observed players
    public static Dictionary<string, Player> onlinePlayers = new Dictionary<string, Player>();
    // first allowed logout time after combat
    public double allowedLogoutTime => lastCombatTime + ((NetworkManagerMMO)NetworkManager.singleton).combatLogoutDelay;
    public double remainingLogoutTime => NetworkTime.time < allowedLogoutTime ? (allowedLogoutTime - NetworkTime.time) : 0;
    // helper variable to remember which skill to use when we walked close enough
    int useSkillWhenCloser = -1;

    // networkbehaviour ////////////////////////////////////////////////////////
    protected override void Awake()
    {
        // cache base components
        base.Awake();

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "Awake_");
    }
    protected override void Start()
    {
        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        base.Start();
        onlinePlayers[name] = this;

        // spawn effects for any buffs that might still be active after loading
        // (OnStartServer is too early)
        // note: no need to do that in Entity.Start because we don't load them
        //       with previously casted skills
        if (isServer)
            for (int i = 0; i < buffs.Count; ++i)
                if (buffs[i].BuffTimeRemaining() > 0)
                    buffs[i].data.SpawnEffect(this, this);

        // notify guild members that we are online. this also updates the client's
        // own guild info via targetrpc automatically
        // -> OnStartServer is too early because it's not spawned there yet
        if (isServer)
            SetGuildOnline(true);

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "Start_");
    }
    private void LateUpdate()
    {
        // pass parameters to animation state machine
        // => passing the states directly is the most reliable way to avoid all
        //    kinds of glitches like movement sliding, attack twitching, etc.
        // => make sure to import all looping animations like idle/run/attack
        //    with 'loop time' enabled, otherwise the client might only play it
        //    once
        // => MOVING state is set to local IsMovement result directly. otherwise
        //    we would see animation latencies for rubberband movement if we
        //    have to wait for MOVING state to be received from the server
        // => MOVING checks if !CASTING because there is a case in UpdateMOVING
        //    -> SkillRequest where we still slide to the final position (which
        //    is good), but we should show the casting animation then.
        // => skill names are assumed to be boolean parameters in animator
        //    so we don't need to worry about an animation number etc.
        if (isClient) // no need for animations on the server
        {
            animator.SetBool("MOVING", IsMoving() && State != "CASTING" && !IsMounted());
            animator.SetBool("CASTING", State == "CASTING");
            foreach (Skill skill in skills)
                if (skill.level > 0 && !(skill.data is PassiveSkill))
                    animator.SetBool(skill.name, skill.CastTimeRemaining() > 0);
            animator.SetBool("STUNNED", State == "STUNNED");
            animator.SetBool("DEAD", State == "DEAD");
            animator.SetFloat("LookX", lookDirection.x);
            animator.SetFloat("LookY", lookDirection.y);
        }

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "LateUpdate_");
    }
    private void OnDestroy()
    {
        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        // Unity bug: isServer is false when called in host mode. only true when
        // called in dedicated mode. so we need a workaround:
        if (NetworkServer.active) // isServer
        {
            // leave party (if any)
            if (InParty())
            {
                // dismiss if master, leave otherwise
                if (party.master == name)
                    PartyDismiss();
                else
                    PartyLeave();
            }

            // notify guild members that we are offline
            SetGuildOnline(false);
        }

        if (isLocalPlayer) // requires at least Unity 5.5.1 bugfix to work
        {
            Destroy(indicator);
            SaveSkillbar();
            localPlayer = null;
        }

        onlinePlayers.Remove(name);

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "OnDestroy_");
    }

    public override void OnStartLocalPlayer()
    {
        // set singleton
        localPlayer = this;

        // make camera follow the local player. we don't just set .parent
        // because the player might be destroyed, but the camera never should be
        Camera.main.GetComponent<CameraMMO2D>().target = transform;
        GameObject.FindWithTag("MinimapCamera").GetComponent<CopyPosition>().target = transform;
        if (avatarCamera) avatarCamera.enabled = true; // avatar camera for local player

        // load skillbar after player data was loaded
        LoadSkillbar();

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "OnStartLocalPlayer_");
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        // setup synclist callbacks on client. no need to update and show and
        // animate equipment on server
        equipment.Callback += OnEquipmentChanged;

        // refresh all locations once (on synclist changed won't be called
        // for initial lists)
        // -> needs to happen before ProximityChecker's initial SetVis call,
        //    otherwise we get a hidden character with visible equipment
        //    (hence OnStartClient and not Start)
        for (int i = 0; i < equipment.Count; ++i)
            RefreshLocation(i);
    }
    public override void OnStartServer()
    {
        base.OnStartServer();

        // initialize trade item indices
        for (int i = 0; i < 6; ++i) tradeOfferItems.Add(-1);

        InvokeRepeating(nameof(ProcessCoinOrders), 5, 5);

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "OnStartServer_");
    }

    // finite state machine events - status based //////////////////////////////
    // status based events
    private bool EventDied()
    {
        return Health == 0;
    }
    private bool EventTargetDisappeared()
    {
        return Target == null;
    }
    private bool EventTargetDied()
    {
        return Target != null && Target.Health == 0;
    }
    private bool EventSkillRequest()
    {
        return 0 <= currentSkill && currentSkill < skills.Count;
    }
    private bool EventSkillFinished()
    {
        return 0 <= currentSkill && currentSkill < skills.Count &&
               skills[currentSkill].CastTimeRemaining() == 0;
    }
    private bool EventMoveStart()
    {
        return State != "MOVING" && IsMoving(); // only fire when started moving
    }
    private bool EventMoveEnd()
    {
        return State == "MOVING" && !IsMoving(); // only fire when stopped moving
    }
    private bool EventTradeStarted()
    {
        // did someone request a trade? and did we request a trade with him too?
        Player player = FindPlayerFromTradeInvitation();
        return player != null && player.tradeRequestFrom == name;
    }
    private bool EventTradeDone()
    {
        // trade canceled or finished?
        return State == "TRADING" && tradeRequestFrom == "";
    }
    private bool craftingRequested;
    private bool EventCraftingStarted()
    {
        bool result = craftingRequested;
        craftingRequested = false;
        return result;
    }
    private bool EventCraftingDone()
    {
        return State == "CRAFTING" && NetworkTime.time > craftingTimeEnd;
    }
    private bool EventStunned()
    {
        return NetworkTime.time <= stunTimeEnd;
    }

    // finite state machine events - command based /////////////////////////////
    // client calls command, command sets a flag, event reads and resets it
    // => we use a set so that we don't get ultra long queues etc.
    // => we use set.Return to read and clear values
    HashSet<string> cmdEvents = new HashSet<string>();

    [Command]
    public void CmdRespawn()
    {
        cmdEvents.Add("Respawn");
    }
    private bool EventRespawn()
    {
        return cmdEvents.Remove("Respawn");
    }

    [Command]
    public void CmdCancelAction()
    {
        cmdEvents.Add("CancelAction");
    }
    private bool EventCancelAction()
    {
        return cmdEvents.Remove("CancelAction");
    }

    // finite state machine - server ///////////////////////////////////////////
    [Server]
    private string UpdateServer_IDLE()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died.
            OnDeath();
            return "DEAD";
        }
        if (EventStunned())
        {
            rubberbanding.ResetMovement();
            return "STUNNED";
        }
        if (EventCancelAction())
        {
            // the only thing that we can cancel is the target
            Target = null;
            return "IDLE";
        }
        if (EventTradeStarted())
        {
            // cancel casting (if any), set target, go to trading
            currentSkill = -1; // just in case
            Target = FindPlayerFromTradeInvitation();
            return "TRADING";
        }
        if (EventCraftingStarted())
        {
            // cancel casting (if any), go to crafting
            currentSkill = -1; // just in case
            return "CRAFTING";
        }
        if (EventMoveStart())
        {
            // cancel casting (if any)
            currentSkill = -1;
            return "MOVING";
        }
        if (EventSkillRequest())
        {
            // don't cast while mounted
            // (no MOUNTED state because we'd need MOUNTED_STUNNED, etc. too)
            if (!IsMounted())
            {
                // user wants to cast a skill.
                // check self (alive, mana, weapon etc.) and target and distance
                Skill skill = skills[currentSkill];
                NextTarget = Target; // return to this one after any corrections by CastCheckTarget
                Vector2 destination;
                if (CastCheckSelf(skill) && CastCheckTarget(skill) && CastCheckDistance(skill, out destination))
                {
                    // start casting and cancel movement in any case
                    // (player might move into attack range * 0.8 but as soon as we
                    //  are close enough to cast, we fully commit to the cast.)
                    rubberbanding.ResetMovement();
                    StartCastSkill(skill);
                    return "CASTING";
                }
                else
                {
                    // checks failed. stop trying to cast.
                    currentSkill = -1;
                    NextTarget = null; // nevermind, clear again (otherwise it's shown in UITarget)
                    return "IDLE";
                }
            }
        }
        if (EventSkillFinished()) { } // don't care
        if (EventMoveEnd()) { } // don't care
        if (EventTradeDone()) { } // don't care
        if (EventCraftingDone()) { } // don't care
        if (EventRespawn()) { } // don't care
        if (EventTargetDied()) { } // don't care
        if (EventTargetDisappeared()) { } // don't care

        return "IDLE"; // nothing interesting happened
    }

    [Server]
    private string UpdateServer_MOVING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died.
            OnDeath();
            return "DEAD";
        }
        if (EventStunned())
        {
            rubberbanding.ResetMovement();
            return "STUNNED";
        }
        if (EventMoveEnd())
        {
            // finished moving. do whatever we did before.
            return "IDLE";
        }
        if (EventCancelAction())
        {
            // cancel casting (if any) and stop moving
            currentSkill = -1;
            //rubberbanding.ResetMovement(); <- done locally. doing it here would reset localplayer to the slightly behind server position otherwise
            return "IDLE";
        }
        if (EventTradeStarted())
        {
            // cancel casting (if any), stop moving, set target, go to trading
            currentSkill = -1;
            rubberbanding.ResetMovement();
            Target = FindPlayerFromTradeInvitation();
            return "TRADING";
        }
        if (EventCraftingStarted())
        {
            // cancel casting (if any), stop moving, go to crafting
            currentSkill = -1;
            rubberbanding.ResetMovement();
            return "CRAFTING";
        }
        // SPECIAL CASE: Skill Request while doing rubberband movement
        // -> we don't really need to react to it
        // -> we could just wait for move to end, then react to request in IDLE
        // -> BUT player position on server always lags behind in rubberband movement
        // -> SO there would be a noticeable delay before we start to cast
        //
        // SOLUTION:
        // -> start casting as soon as we are in range
        // -> BUT don't ResetMovement. instead let it slide to the final position
        //    while already starting to cast
        // -> NavMeshAgentRubberbanding won't accept new positions while casting
        //    anyway, so this is fine
        if (EventSkillRequest())
        {
            // don't cast while mounted
            // (no MOUNTED state because we'd need MOUNTED_STUNNED, etc. too)
            if (!IsMounted())
            {
                Vector2 destination;
                Skill skill = skills[currentSkill];
                if (CastCheckSelf(skill) && CastCheckTarget(skill) && CastCheckDistance(skill, out destination))
                {
                    //Debug.Log("MOVING->EventSkillRequest: early cast started while sliding to destination...");
                    // rubberbanding.ResetMovement(); <- DO NOT DO THIS.
                    StartCastSkill(skill);
                    return "CASTING";
                }
            }
        }
        if (EventMoveStart()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventTradeDone()) { } // don't care
        if (EventCraftingDone()) { } // don't care
        if (EventRespawn()) { } // don't care
        if (EventTargetDied()) { } // don't care
        if (EventTargetDisappeared()) { } // don't care

        return "MOVING"; // nothing interesting happened
    }

    private void UseNextTargetIfAny()
    {
        // use next target if the user tried to target another while casting
        // (target is locked while casting so skill isn't applied to an invalid
        //  target accidentally)
        if (NextTarget != null)
        {
            Target = NextTarget;
            NextTarget = null;
        }
    }

    [Server]
    private string UpdateServer_CASTING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        //
        // IMPORTANT: nextTarget might have been set while casting, so make sure
        // to handle it in any case here. it should definitely be null again
        // after casting was finished.
        // => this way we can reliably display nextTarget on the client if it's
        //    != null, so that UITarget always shows nextTarget>target
        //    (this just feels better)
        if (EventDied())
        {
            // we died.
            OnDeath();
            UseNextTargetIfAny(); // if user selected a new target while casting
            return "DEAD";
        }
        if (EventStunned())
        {
            currentSkill = -1;
            rubberbanding.ResetMovement();
            return "STUNNED";
        }
        if (EventMoveStart())
        {
            // we do NOT cancel the cast if the player moved, and here is why:
            // * local player might move into cast range and then try to cast.
            // * server then receives the Cmd, goes to CASTING state, then
            //   receives one of the last movement updates from the local player
            //   which would cause EventMoveStart and cancel the cast.
            // * this is the price for rubberband movement.
            // => if the player wants to cast and got close enough, then we have
            //    to fully commit to it. there is no more way out except via
            //    cancel action. any movement in here is to be rejected.
            //    (many popular MMOs have the same behaviour too)
            //
            // we do NOT reset movement either. allow sliding to final position.
            // (NavMeshAgentRubberbanding doesn't accept new ones while CASTING)
            //rubberbanding.ResetMovement(); <- DO NOT DO THIS
            return "CASTING";
        }
        if (EventCancelAction())
        {
            // cancel casting
            currentSkill = -1;
            UseNextTargetIfAny(); // if user selected a new target while casting
            return "IDLE";
        }
        if (EventTradeStarted())
        {
            // cancel casting (if any), stop moving, set target, go to trading
            currentSkill = -1;
            rubberbanding.ResetMovement();

            // set target to trade target instead of next target (clear that)
            Target = FindPlayerFromTradeInvitation();
            NextTarget = null;
            return "TRADING";
        }
        if (EventTargetDisappeared())
        {
            // cancel if the target matters for this skill
            if (skills[currentSkill].cancelCastIfTargetDied)
            {
                currentSkill = -1;
                UseNextTargetIfAny(); // if user selected a new target while casting
                return "IDLE";
            }
        }
        if (EventTargetDied())
        {
            // cancel if the target matters for this skill
            if (skills[currentSkill].cancelCastIfTargetDied)
            {
                currentSkill = -1;
                UseNextTargetIfAny(); // if user selected a new target while casting
                return "IDLE";
            }
        }
        if (EventSkillFinished())
        {
            // apply the skill after casting is finished
            // note: we don't check the distance again. it's more fun if players
            //       still cast the skill if the target ran a few steps away
            Skill skill = skills[currentSkill];

            // apply the skill on the target
            FinishCastSkill(skill);

            // clear current skill for now
            currentSkill = -1;

            // target-based skill and no more valid target? then clear
            // (otherwise IDLE will get an unnecessary skill request and mess
            //  with targeting)
            bool validTarget = Target != null && Target.Health > 0;
            if (currentSkill != -1 && skills[currentSkill].cancelCastIfTargetDied && !validTarget)
                currentSkill = -1;

            // use next target if the user tried to target another while casting
            UseNextTargetIfAny();

            // go back to IDLE
            return "IDLE";
        }
        if (EventMoveEnd()) { } // don't care
        if (EventTradeDone()) { } // don't care
        if (EventCraftingStarted()) { } // don't care
        if (EventCraftingDone()) { } // don't care
        if (EventRespawn()) { } // don't care
        if (EventSkillRequest()) { } // don't care

        return "CASTING"; // nothing interesting happened
    }

    [Server]
    private string UpdateServer_STUNNED()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died.
            OnDeath();
            return "DEAD";
        }
        if (EventStunned())
        {
            return "STUNNED";
        }

        // go back to idle if we aren't stunned anymore and process all new
        // events there too
        return "IDLE";
    }

    [Server]
    private string UpdateServer_TRADING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died, stop trading. other guy will receive targetdied event.
            OnDeath();
            TradeCleanup();
            return "DEAD";
        }
        if (EventStunned())
        {
            // stop trading
            currentSkill = -1;
            rubberbanding.ResetMovement();
            TradeCleanup();
            return "STUNNED";
        }
        if (EventMoveStart())
        {
            // reject movement while trading
            rubberbanding.ResetMovement();
            return "TRADING";
        }
        if (EventCancelAction())
        {
            // stop trading
            TradeCleanup();
            return "IDLE";
        }
        if (EventTargetDisappeared())
        {
            // target disconnected, stop trading
            TradeCleanup();
            return "IDLE";
        }
        if (EventTargetDied())
        {
            // target died, stop trading
            TradeCleanup();
            return "IDLE";
        }
        if (EventTradeDone())
        {
            // someone canceled or we finished the trade. stop trading
            TradeCleanup();
            return "IDLE";
        }
        if (EventMoveEnd()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventCraftingStarted()) { } // don't care
        if (EventCraftingDone()) { } // don't care
        if (EventRespawn()) { } // don't care
        if (EventTradeStarted()) { } // don't care
        if (EventSkillRequest()) { } // don't care

        return "TRADING"; // nothing interesting happened
    }

    [Server]
    private string UpdateServer_CRAFTING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died, stop crafting
            OnDeath();
            return "DEAD";
        }
        if (EventStunned())
        {
            // stop crafting
            rubberbanding.ResetMovement();
            return "STUNNED";
        }
        if (EventMoveStart())
        {
            // reject movement while crafting
            rubberbanding.ResetMovement();
            return "CRAFTING";
        }
        if (EventCraftingDone())
        {
            // finish crafting
            Craft();
            return "IDLE";
        }
        if (EventCancelAction()) { } // don't care. user pressed craft, we craft.
        if (EventTargetDisappeared()) { } // don't care
        if (EventTargetDied()) { } // don't care
        if (EventMoveEnd()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventRespawn()) { } // don't care
        if (EventTradeStarted()) { } // don't care
        if (EventTradeDone()) { } // don't care
        if (EventCraftingStarted()) { } // don't care
        if (EventSkillRequest()) { } // don't care

        return "CRAFTING"; // nothing interesting happened
    }

    [Server]
    private string UpdateServer_DEAD()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventRespawn())
        {
            // revive to closest spawn, with 50% health, then go to idle
            Transform start = NetworkManagerMMO.GetNearestStartPosition(transform.position);
            agent.Warp(start.position); // recommended over transform.position
            Revive(0.5f);
            return "IDLE";
        }
        if (EventMoveStart())
        {
            // this should never happen, rubberband should prevent from moving
            // while dead.
            Debug.LogWarning("Player " + name + " moved while dead. This should not happen.");
            return "DEAD";
        }
        if (EventMoveEnd()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventDied()) { } // don't care
        if (EventCancelAction()) { } // don't care
        if (EventTradeStarted()) { } // don't care
        if (EventTradeDone()) { } // don't care
        if (EventCraftingStarted()) { } // don't care
        if (EventCraftingDone()) { } // don't care
        if (EventTargetDisappeared()) { } // don't care
        if (EventTargetDied()) { } // don't care
        if (EventSkillRequest()) { } // don't care

        return "DEAD"; // nothing interesting happened
    }

    [Server]
    protected override string UpdateServer()
    {
        if (State == "IDLE") return UpdateServer_IDLE();
        if (State == "MOVING") return UpdateServer_MOVING();
        if (State == "CASTING") return UpdateServer_CASTING();
        if (State == "STUNNED") return UpdateServer_STUNNED();
        if (State == "TRADING") return UpdateServer_TRADING();
        if (State == "CRAFTING") return UpdateServer_CRAFTING();
        if (State == "DEAD") return UpdateServer_DEAD();
        Debug.LogError("invalid state:" + State);
        return "IDLE";
    }

    // finite state machine - client ///////////////////////////////////////////
    [Client]
    [Obsolete]
    protected override void UpdateClient()
    {
        if (State == "IDLE" || State == "MOVING")
        {
            if (isLocalPlayer)
            {
                // simply accept input
                SelectionHandling();
                WASDHandling();
                TargetNearest();

                // cancel action if escape key was pressed
                if (Input.GetKeyDown(cancelActionKey))
                {
                    agent.ResetPath(); // reset locally because we use rubberband movement
                    CmdCancelAction();
                }

                // trying to cast a skill on a monster that wasn't in range?
                // then check if we walked into attack range by now
                if (useSkillWhenCloser != -1)
                {
                    // can we still attack the target? maybe it was switched.
                    if (CanAttack(Target))
                    {
                        // in range already?
                        // -> we don't use CastCheckDistance because we want to
                        // move a bit closer (attackToMoveRangeRatio)
                        float range = skills[useSkillWhenCloser].castRange * attackToMoveRangeRatio;
                        if (Utils.ClosestDistance(collider, Target.collider) <= range)
                        {
                            // then stop moving and start attacking
                            CmdUseSkill(useSkillWhenCloser, lookDirection);

                            // reset
                            useSkillWhenCloser = -1;
                        }
                        // otherwise keep walking there. the target might move
                        // around or run away, so we need to keep adjusting the
                        // destination all the time
                        else
                        {
                            //Debug.Log("walking closer to target...");
                            agent.stoppingDistance = range;
                            agent.destination = Target.collider.ClosestPointOnBounds(transform.position);
                        }
                    }
                    // otherwise reset
                    else useSkillWhenCloser = -1;
                }
            }
        }
        else if (State == "CASTING")
        {
            if (isLocalPlayer)
            {
                // simply accept input and reset any client sided movement
                SelectionHandling();
                WASDHandling(); // still call this to set pendingVelocity for after cast
                TargetNearest();
                agent.ResetMovement();

                // cancel action if escape key was pressed
                if (Input.GetKeyDown(cancelActionKey)) CmdCancelAction();
            }
        }
        else if (State == "STUNNED")
        {
            if (isLocalPlayer)
            {
                // simply accept input and reset any client sided movement
                SelectionHandling();
                TargetNearest();
                agent.ResetMovement();

                // cancel action if escape key was pressed
                if (Input.GetKeyDown(cancelActionKey)) CmdCancelAction();
            }
        }
        else if (State == "TRADING") { }
        else if (State == "CRAFTING") { }
        else if (State == "DEAD") { }
        else Debug.LogError("invalid state:" + State);

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "UpdateClient_");
    }

    // overlays ////////////////////////////////////////////////////////////////
    protected override void UpdateOverlays()
    {
        base.UpdateOverlays();

        if (panName != null)
        {
            // find local player (null while in character selection)
            if (localPlayer != null)
            {
                // note: murderer has higher priority (a player can be a murderer and an
                // offender at the same time)
                if (IsMurderer())
                    panName.GetComponentInChildren<Text>().color = clrMurderer;
                else if (IsOffender())
                    panName.GetComponentInChildren<Text>().color = clrOffender;
                // member of the same party
                else if (localPlayer.InParty() && localPlayer.party.Contains(name))
                    panName.GetComponentInChildren<Text>().color = clrParty;
                // otherwise default
                else
                    panName.GetComponentInChildren<Text>().color = clrDefault;
            }
        }
        if (panGuild != null)
        {
            panGuild.SetActive(true);
            panGuild.GetComponentInChildren<Text>().text = InGuild() ? guildPrefix + guild.name + guildSuffix : "";
        }
    }

    // skill finished event & pending actions //////////////////////////////////
    // pending actions while casting. to be applied after cast.
    int pendingSkill = -1;
    Vector2 pendingDestination;
    bool pendingDestinationValid;
    Vector3 pendingVelocity;
    bool pendingVelocityValid;

    // client event when skill cast finished on server
    // -> useful for follow up attacks etc.
    //    (doing those on server won't really work because the target might have
    //     moved, in which case we need to follow, which we need to do on the
    //     client)
    [Client]
    void OnSkillCastFinished(Skill skill)
    {
        if (!isLocalPlayer) return;

        // tried to click move somewhere?
        if (pendingDestinationValid)
        {
            agent.stoppingDistance = 0;
            agent.destination = pendingDestination;
        }
        // tried to wasd move somewhere?
        else if (pendingVelocityValid)
        {
            agent.velocity = pendingVelocity;
        }
        // user pressed another skill button?
        else if (pendingSkill != -1)
        {
            TryUseSkill(pendingSkill, true);
        }
        // otherwise do follow up attack if no interruptions happened
        else if (skill.followupDefaultAttack)
        {
            TryUseSkill(0, true);
        }

        // clear pending actions in any case
        pendingSkill = -1;
        pendingDestinationValid = false;
        pendingVelocityValid = false;
    }

    // attributes //////////////////////////////////////////////////////////////
    public static LinearInt AttributesSpendablePerLevel = new LinearInt { baseValue = 1, bonusPerLevel = 1 };

    /// <summary>
    /// 计算剩余可支配点数
    /// </summary>
    /// <returns>剩余可支配点数</returns>
    public int AttributesSpendable()
    {
        return (AttributesSpendablePerLevel.Get(level)) -
            (healthAdditional + mindAdditional + powerAdditional + strengthAdditional + defenseAdditional + runSpeedMultipleAdditional + intelligenceAdditional);
    }

    [Command]
    public void CmdIncreaseStrength()
    {
        // validate
        if (Health > 0 && AttributesSpendable() > 0) ++healthAdditional;
    }

    [Command]
    public void CmdIncreaseIntelligence()
    {
        // validate
        if (Health > 0 && AttributesSpendable() > 0) ++intelligenceAdditional;
    }

    // combat //////////////////////////////////////////////////////////////////
    // helper function to calculate the experience rewards for sharing parties
    public static long CalculatePartyExperienceShare(long total, int memberCount, float bonusPercentagePerMember, int memberLevel, int killedLevel)
    {
        // bonus percentage based on how many members there are
        float bonusPercentage = (memberCount - 1) * bonusPercentagePerMember;

        // calculate the share via ceil, so that uneven numbers still result in
        // at least 'total' in the end. for example:
        //   4/2=2 (good)
        //   5/2=2 (bad. 1 point got lost)
        //   ceil(5/(float)2) = 3 (good!)
        long share = (long)Mathf.Ceil(total / (float)memberCount);

        // balance experience reward for the receiver's level. this is important
        // to avoid crazy power leveling where a level 1 hero would get a LOT of
        // level ups if his friend kills a level 100 monster once.
        long balanced = BalanceExpReward(share, memberLevel, killedLevel);
        long bonus = Convert.ToInt64(balanced * bonusPercentage);

        return balanced + bonus;
    }

    [Server]
    public void OnDamageDealtToMonster(Monster monster)
    {
        // did we kill it?
        if (monster.Health == 0)
        {
            // share kill rewards with party or only for self
            List<Player> closeMembers = InParty() ? GetPartyMembersInProximity() : new List<Player>();

            // share experience & skill experience
            // note: bonus only applies to exp. share parties, otherwise
            //       there's an unnecessary pressure to always join a
            //       party when leveling alone too.
            // note: if monster.rewardExp is 10 then it's possible that
            //       two members only receive 2 exp each (= 4 total).
            //       this happens because of exp balancing by level and
            //       is as intended.
            if (InParty() && party.shareExperience)
            {
                foreach (Player member in closeMembers)
                {
                    member.Experience += CalculatePartyExperienceShare(
                        monster.rewardExperience,
                        closeMembers.Count,
                        Party.BonusExperiencePerMember,
                        member.level,
                        monster.level
                    );
                    member.skillExperience += CalculatePartyExperienceShare(
                        monster.rewardSkillExperience,
                        closeMembers.Count,
                        Party.BonusExperiencePerMember,
                        member.level,
                        monster.level
                    );
                }
            }
            else
            {
                skillExperience += BalanceExpReward(monster.rewardSkillExperience, level, monster.level);
                Experience += BalanceExpReward(monster.rewardExperience, level, monster.level);
            }

            // give pet the same exp without dividing it, but balance it
            // => AFTER player exp reward! pet can only ever level up to player
            //    level, so it's best if the player gets exp and level-ups
            //    first, then afterwards we try to level up the pet.
            if (ActivePet != null)
                ActivePet.Experience += BalanceExpReward(monster.rewardExperience, ActivePet.level, monster.level);

            // increase quest kill counter for all party members
            if (InParty())
            {
                foreach (Player member in closeMembers)
                    member.QuestsOnKilled(monster);
            }
            else QuestsOnKilled(monster);
        }
    }

    [Server]
    public void OnDamageDealtToPlayer(Player player)
    {
        // was he innocent?
        if (!player.IsOffender() && !player.IsMurderer())
        {
            // did we kill him? then start/reset murder status
            // did we just attack him? then start/reset offender status
            // (unless we are already a murderer)
            if (player.Health == 0) StartMurderer();
            else if (!IsMurderer()) StartOffender();
        }
    }

    [Server]
    public void OnDamageDealtToPet(Pet pet)
    {
        // was he innocent?
        if (!pet.owner.IsOffender() && !pet.owner.IsMurderer())
        {
            // did we kill him? then start/reset murder status
            // did we just attack him? then start/reset offender status
            // (unless we are already a murderer)
            if (pet.Health == 0) StartMurderer();
            else if (!IsMurderer()) StartOffender();
        }
    }

    // custom DealDamageAt function that also rewards experience if we killed
    // the monster
    [Server]
    public override void DealDamageAt(Entity entity, int amount, float stunChance = 0, float stunTime = 0)
    {
        // deal damage with the default function
        base.DealDamageAt(entity, amount, stunChance, stunTime);

        // a monster?
        if (entity is Monster)
        {
            OnDamageDealtToMonster((Monster)entity);
        }
        // a player?
        // (see murder code section comments to understand the system)
        else if (entity is Player)
        {
            OnDamageDealtToPlayer((Player)entity);
        }
        // a pet?
        // (see murder code section comments to understand the system)
        else if (entity is Pet)
        {
            OnDamageDealtToPet((Pet)entity);
        }

        // let pet know that we attacked something
        if (ActivePet != null && ActivePet.autoAttack)
            ActivePet.OnAggro(entity);

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "DealDamageAt_", entity, amount);
    }

    // experience //////////////////////////////////////////////////////////////
    public float ExperiencePercent()
    {
        return (Experience != 0 && ExperienceMax != 0) ? (float)Experience / (float)ExperienceMax : 0;
    }

    // players gain exp depending on their level. if a player has a lower level
    // than the monster, then he gains more exp (up to 100% more) and if he has
    // a higher level, then he gains less exp (up to 100% less)
    // -> test with monster level 20 and expreward of 100:
    //   BalanceExpReward( 1, 20, 100)); => 200
    //   BalanceExpReward( 9, 20, 100)); => 200
    //   BalanceExpReward(10, 20, 100)); => 200
    //   BalanceExpReward(11, 20, 100)); => 190
    //   BalanceExpReward(12, 20, 100)); => 180
    //   BalanceExpReward(13, 20, 100)); => 170
    //   BalanceExpReward(14, 20, 100)); => 160
    //   BalanceExpReward(15, 20, 100)); => 150
    //   BalanceExpReward(16, 20, 100)); => 140
    //   BalanceExpReward(17, 20, 100)); => 130
    //   BalanceExpReward(18, 20, 100)); => 120
    //   BalanceExpReward(19, 20, 100)); => 110
    //   BalanceExpReward(20, 20, 100)); => 100
    //   BalanceExpReward(21, 20, 100)); =>  90
    //   BalanceExpReward(22, 20, 100)); =>  80
    //   BalanceExpReward(23, 20, 100)); =>  70
    //   BalanceExpReward(24, 20, 100)); =>  60
    //   BalanceExpReward(25, 20, 100)); =>  50
    //   BalanceExpReward(26, 20, 100)); =>  40
    //   BalanceExpReward(27, 20, 100)); =>  30
    //   BalanceExpReward(28, 20, 100)); =>  20
    //   BalanceExpReward(29, 20, 100)); =>  10
    //   BalanceExpReward(30, 20, 100)); =>   0
    //   BalanceExpReward(31, 20, 100)); =>   0
    public static long BalanceExpReward(long reward, int attackerLevel, int victimLevel)
    {
        int levelDiff = Mathf.Clamp(victimLevel - attackerLevel, -20, 20);
        float multiplier = 1 + levelDiff * 0.1f;
        return Convert.ToInt64(reward * multiplier);
    }

    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by entities that attack us
    [ServerCallback]
    public override void OnAggro(Entity entity)
    {
        // forward to pet if it's supposed to defend us
        if (ActivePet != null && ActivePet.defendOwner)
            ActivePet.OnAggro(entity);
    }

    // death ///////////////////////////////////////////////////////////////////
    protected override void OnDeath()
    {
        // take care of entity stuff
        base.OnDeath();

        // rubberbanding needs a custom reset
        rubberbanding.ResetMovement();

        // lose experience
        long loss = Convert.ToInt64(ExperienceMax * deathExperienceLossPercent);
        Experience -= loss;

        // send an info chat message
        string message = "You died and lost " + loss + " experience.";
        chat.TargetMsgInfo(message);

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "OnDeath_");
    }

    // loot ////////////////////////////////////////////////////////////////////
    [Command]
    public void CmdTakeLootGold()
    {
        // validate: dead monster and close enough?
        // use collider point(s) to also work with big entities
        if ((State == "IDLE" || State == "MOVING" || State == "CASTING") &&
            Target != null && Target is Monster && Target.Health == 0 &&
            Utils.ClosestDistance(collider, Target.collider) <= interactionRange)
        {
            // distribute reward through party or to self
            if (InParty() && party.shareGold)
            {
                // find all party members in observer range
                // (we don't distribute it all across the map. standing
                //  next to each other is a better experience. players
                //  can't just stand safely in a city while gaining exp)
                List<Player> closeMembers = GetPartyMembersInProximity();

                // calculate the share via ceil, so that uneven numbers
                // still result in at least total gold in the end.
                // e.g. 4/2=2 (good); 5/2=2 (1 gold got lost)
                long share = (long)Mathf.Ceil((float)Target.Money / (float)closeMembers.Count);

                // now distribute
                foreach (Player member in closeMembers)
                    member.Money += share;
            }
            else
            {
                Money += Target.Money;
            }

            // reset target gold
            Target.Money = 0;
        }
    }

    [Command]
    public void CmdTakeLootItem(int index)
    {
        // validate: dead monster and close enough and valid loot index?
        // use collider point(s) to also work with big entities
        if ((State == "IDLE" || State == "MOVING" || State == "CASTING") &&
            Target != null && Target is Monster && Target.Health == 0 &&
            Utils.ClosestDistance(collider, Target.collider) <= interactionRange &&
            0 <= index && index < Target.inventory.Count &&
            Target.inventory[index].amount > 0)
        {
            ItemSlot slot = Target.inventory[index];

            // try to add it to the inventory, clear monster slot if it worked
            if (InventoryAdd(slot.item, slot.amount))
            {
                slot.amount = 0;
                Target.inventory[index] = slot;
            }
        }
    }

    // inventory ///////////////////////////////////////////////////////////////
    // are inventory operations like swap, merge, split allowed at the moment?
    bool InventoryOperationsAllowed()
    {
        return State == "IDLE" ||
               State == "MOVING" ||
               State == "CASTING" ||
               (State == "TRADING" && tradeStatus == TradeStatus.Free);
    }

    [Command]
    public void CmdSwapInventoryTrash(int inventoryIndex)
    {
        // dragging an inventory item to the trash always overwrites the trash
        if (InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.Count)
        {
            // inventory slot has to be valid and destroyable and not summoned
            ItemSlot slot = inventory[inventoryIndex];
            if (slot.amount > 0 && slot.item.destroyable && !slot.item.summoned)
            {
                // overwrite trash
                trash = slot;

                // clear inventory slot
                slot.amount = 0;
                inventory[inventoryIndex] = slot;
            }
        }
    }

    [Command]
    public void CmdSwapTrashInventory(int inventoryIndex)
    {
        if (InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.Count)
        {
            // inventory slot has to be empty or destroyable
            ItemSlot slot = inventory[inventoryIndex];
            if (slot.amount == 0 || slot.item.destroyable)
            {
                // swap them
                inventory[inventoryIndex] = trash;
                trash = slot;
            }
        }
    }

    [Command]
    public void CmdSwapInventoryInventory(int fromIndex, int toIndex)
    {
        // note: should never send a command with complex types!
        // validate: make sure that the slots actually exist in the inventory
        // and that they are not equal
        if (InventoryOperationsAllowed() &&
            0 <= fromIndex && fromIndex < inventory.Count &&
            0 <= toIndex && toIndex < inventory.Count &&
            fromIndex != toIndex)
        {
            // swap them
            ItemSlot temp = inventory[fromIndex];
            inventory[fromIndex] = inventory[toIndex];
            inventory[toIndex] = temp;
        }
    }

    [Command]
    public void CmdInventorySplit(int fromIndex, int toIndex)
    {
        // note: should never send a command with complex types!
        // validate: make sure that the slots actually exist in the inventory
        // and that they are not equal
        if (InventoryOperationsAllowed() &&
            0 <= fromIndex && fromIndex < inventory.Count &&
            0 <= toIndex && toIndex < inventory.Count &&
            fromIndex != toIndex)
        {
            // slotFrom needs at least two to split, slotTo has to be empty
            ItemSlot slotFrom = inventory[fromIndex];
            ItemSlot slotTo = inventory[toIndex];
            if (slotFrom.amount >= 2 && slotTo.amount == 0)
            {
                // split them serversided (has to work for even and odd)
                slotTo = slotFrom; // copy the value

                slotTo.amount = slotFrom.amount / 2;
                slotFrom.amount -= slotTo.amount; // works for odd too

                // put back into the list
                inventory[fromIndex] = slotFrom;
                inventory[toIndex] = slotTo;
            }
        }
    }

    [Command]
    public void CmdInventoryMerge(int fromIndex, int toIndex)
    {
        if (InventoryOperationsAllowed() &&
            0 <= fromIndex && fromIndex < inventory.Count &&
            0 <= toIndex && toIndex < inventory.Count &&
            fromIndex != toIndex)
        {
            // both items have to be valid
            ItemSlot slotFrom = inventory[fromIndex];
            ItemSlot slotTo = inventory[toIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                // make sure that items are the same type
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slotFrom.item.Equals(slotTo.item))
                {
                    // merge from -> to
                    // put as many as possible into 'To' slot
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);

                    // put back into the list
                    inventory[fromIndex] = slotFrom;
                    inventory[toIndex] = slotTo;
                }
            }
        }
    }

    [ClientRpc]
    public void RpcUsedItem(Item item)
    {
        // validate
        if (item.data is UsableItem)
        {
            UsableItem itemData = (UsableItem)item.data;
            itemData.OnUsed(this);
        }
    }

    [Command]
    public void CmdUseInventoryItem(int index)
    {
        // validate
        if (InventoryOperationsAllowed() &&
            0 <= index && index < inventory.Count && inventory[index].amount > 0 &&
            inventory[index].item.data is UsableItem)
        {
            // use item
            // note: we don't decrease amount / destroy in all cases because
            // some items may swap to other slots in .Use()
            UsableItem itemData = (UsableItem)inventory[index].item.data;
            if (itemData.CanUse(this, index))
            {
                // .Use might clear the slot, so we backup the Item first for the Rpc
                Item item = inventory[index].item;
                itemData.Use(this, index);
                RpcUsedItem(item);
            }
        }
    }

    // equipment ///////////////////////////////////////////////////////////////
    void OnEquipmentChanged(SyncListItemSlot.Operation op, int index, ItemSlot slot)
    {
        // update the equipment
        RefreshLocation(index);
    }

    public void RefreshLocation(int index)
    {
        ItemSlot slot = equipment[index];
        EquipmentInfo info = equipmentInfo[index];

        // valid cateogry and valid location? otherwise don't bother
        if (info.requiredCategory != "" && info.location != null)
            info.location.spritesToAnimate = slot.amount > 0 ? ((EquipmentItem)slot.item.data).sprites : null;
    }

    [Server]
    public void SwapInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (Health > 0 &&
            0 <= inventoryIndex && inventoryIndex < inventory.Count &&
            0 <= equipmentIndex && equipmentIndex < equipment.Count)
        {
            // item slot has to be empty (unequip) or equipable
            ItemSlot slot = inventory[inventoryIndex];
            if (slot.amount == 0 ||
                slot.item.data is EquipmentItem &&
                ((EquipmentItem)slot.item.data).CanEquip(this, inventoryIndex, equipmentIndex))
            {
                // swap them
                ItemSlot temp = equipment[equipmentIndex];
                equipment[equipmentIndex] = slot;
                inventory[inventoryIndex] = temp;
            }
        }
    }

    [Command]
    public void CmdSwapInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        SwapInventoryEquip(inventoryIndex, equipmentIndex);
    }

    // skills //////////////////////////////////////////////////////////////////
    // we use 'is' instead of 'GetType' so that it works for inherited types too
    public override bool CanAttack(Entity entity)
    {
        return base.CanAttack(entity) &&
               (entity is Monster ||
                entity is Player ||
                (entity is Pet && entity != ActivePet) ||
                (entity is Mount && entity != ActiveMount));

    }

    // always include latest look direction when casting a skill.
    // otherwise if the player makes a tiny move to change the look direction,
    // the server might not register that move and not change the look direction
    // resulting the slash skill to be cast into the wrong direction on the
    // server.
    // => this method is fail safe.
    // => this way we only update look direction when it matters.
    [Command]
    public void CmdUseSkill(int skillIndex, Vector2 direction)
    {
        // validate
        if ((State == "IDLE" || State == "MOVING" || State == "CASTING") &&
            0 <= skillIndex && skillIndex < skills.Count)
        {
            // skill learned and can be casted?
            if (skills[skillIndex].level > 0 && skills[skillIndex].IsReady())
            {
                currentSkill = skillIndex;
                lookDirection = direction;
            }
        }
    }

    // helper function: try to use a skill and walk into range if necessary
    [Client]
    public void TryUseSkill(int skillIndex, bool ignoreState = false)
    {
        // only if not casting already
        // (might need to ignore that when coming from pending skill where
        //  CASTING is still true)
        if (State != "CASTING" || ignoreState)
        {
            Skill skill = skills[skillIndex];
            if (CastCheckSelf(skill) && CastCheckTarget(skill))
            {
                // check distance between self and target
                Vector2 destination;
                if (CastCheckDistance(skill, out destination))
                {
                    // cast
                    CmdUseSkill(skillIndex, lookDirection);
                }
                else
                {
                    // move to the target first
                    // (use collider point(s) to also work with big entities)
                    agent.stoppingDistance = skill.castRange * attackToMoveRangeRatio;
                    agent.destination = destination;

                    // use skill when there
                    useSkillWhenCloser = skillIndex;
                }
            }
        }
        else
        {
            pendingSkill = skillIndex;
        }
    }

    public bool HasLearnedSkill(string skillName)
    {
        return skills.Any(skill => skill.name == skillName && skill.level > 0);
    }

    public bool HasLearnedSkillWithLevel(string skillName, int skillLevel)
    {
        return skills.Any(skill => skill.name == skillName && skill.level >= skillLevel);
    }

    // helper function for command and UI
    // -> this is for learning and upgrading!
    public bool CanUpgradeSkill(Skill skill)
    {
        return skill.level < skill.maxLevel &&
               level >= skill.upgradeRequiredLevel &&
               skillExperience >= skill.upgradeRequiredSkillExperience &&
               (skill.predecessor == null || (HasLearnedSkillWithLevel(skill.predecessor.name, skill.predecessorLevel)));
    }

    // -> this is for learning and upgrading!
    [Command]
    public void CmdUpgradeSkill(int skillIndex)
    {
        // validate
        if ((State == "IDLE" || State == "MOVING" || State == "CASTING") &&
            0 <= skillIndex && skillIndex < skills.Count)
        {
            // can be upgraded?
            Skill skill = skills[skillIndex];
            if (CanUpgradeSkill(skill))
            {
                // decrease skill experience
                skillExperience -= skill.upgradeRequiredSkillExperience;

                // upgrade
                ++skill.level;
                skills[skillIndex] = skill;
            }
        }
    }

    // skillbar ////////////////////////////////////////////////////////////////
    //[Client] <- disabled while UNET OnDestroy isLocalPlayer bug exists
    void SaveSkillbar()
    {
        // save skillbar to player prefs (based on player name, so that
        // each character can have a different skillbar)
        for (int i = 0; i < skillbar.Length; ++i)
            PlayerPrefs.SetString(name + "_skillbar_" + i, skillbar[i].reference);

        // force saving playerprefs, otherwise they aren't saved for some reason
        PlayerPrefs.Save();
    }

    [Client]
    void LoadSkillbar()
    {
        print("loading skillbar for " + name);
        List<Skill> learned = skills.Where(skill => skill.level > 0).ToList();
        for (int i = 0; i < skillbar.Length; ++i)
        {
            // try loading an existing entry
            if (PlayerPrefs.HasKey(name + "_skillbar_" + i))
            {
                string entry = PlayerPrefs.GetString(name + "_skillbar_" + i, "");

                // is this a valid item/equipment/learned skill?
                // (might be an old character's playerprefs)
                // => only allow learned skills (in case it's an old character's
                //    skill that we also have, but haven't learned yet)
                if (HasLearnedSkill(entry) ||
                    GetInventoryIndexByName(entry) != -1 ||
                    GetEquipmentIndexByName(entry) != -1)
                {
                    skillbar[i].reference = entry;
                }
            }
            // otherwise fill with default skills for a better first impression
            else if (i < learned.Count)
            {
                skillbar[i].reference = learned[i].name;
            }
        }
    }

    // quests //////////////////////////////////////////////////////////////////
    public int GetQuestIndexByName(string questName)
    {
        return quests.FindIndex(quest => quest.name == questName);
    }

    // helper function to check if the player has completed a quest before
    public bool HasCompletedQuest(string questName)
    {
        return quests.Any(q => q.name == questName && q.completed);
    }

    // helper function to check if a player has an active (not completed) quest
    public bool HasActiveQuest(string questName)
    {
        return quests.Any(q => q.name == questName && !q.completed);
    }

    [Server]
    public void QuestsOnKilled(Entity victim)
    {
        // call OnKilled in all active (not completed) quests
        for (int i = 0; i < quests.Count; ++i)
            if (!quests[i].completed)
                quests[i].OnKilled(this, i, victim);
    }

    [Server]
    public void QuestsOnLocation(Collider2D location)
    {
        // call OnLocation in all active (not completed) quests
        for (int i = 0; i < quests.Count; ++i)
            if (!quests[i].completed)
                quests[i].OnLocation(this, i, location);
    }

    // helper function to check if the player can accept a new quest
    // note: no quest.completed check needed because we have a'not accepted yet'
    //       check
    public bool CanAcceptQuest(ScriptableQuest quest)
    {
        // not too many quests yet?
        // has required level?
        // not accepted yet?
        // has finished predecessor quest (if any)?
        return quests.Count(q => !q.completed) < activeQuestLimit &&
               level >= quest.requiredLevel &&          // has required level?
               GetQuestIndexByName(quest.name) == -1 && // not accepted yet?
               (quest.predecessor == null || HasCompletedQuest(quest.predecessor.name));
    }

    [Command]
    public void CmdAcceptQuest(int npcQuestIndex)
    {
        // validate
        // use collider point(s) to also work with big entities
        if (State == "IDLE" &&
            Target != null &&
            Target.Health > 0 &&
            Target is Npc &&
            0 <= npcQuestIndex && npcQuestIndex < ((Npc)Target).quests.Length &&
            Utils.ClosestDistance(collider, Target.collider) <= interactionRange)
        {
            ScriptableQuestOffer npcQuest = ((Npc)Target).quests[npcQuestIndex];
            if (npcQuest.acceptHere && CanAcceptQuest(npcQuest.quest))
                quests.Add(new Quest(npcQuest.quest));
        }
    }

    // helper function to check if the player can complete a quest
    public bool CanCompleteQuest(string questName)
    {
        // has the quest and not completed yet?
        int index = GetQuestIndexByName(questName);
        if (index != -1 && !quests[index].completed)
        {
            // fulfilled?
            Quest quest = quests[index];
            if (quest.IsFulfilled(this))
            {
                // enough space for reward item (if any)?
                return quest.rewardItem == null || InventoryCanAdd(new Item(quest.rewardItem), 1);
            }
        }
        return false;
    }

    [Command]
    public void CmdCompleteQuest(int npcQuestIndex)
    {
        // validate
        // use collider point(s) to also work with big entities
        if (State == "IDLE" &&
            Target != null &&
            Target.Health > 0 &&
            Target is Npc &&
            0 <= npcQuestIndex && npcQuestIndex < ((Npc)Target).quests.Length &&
            Utils.ClosestDistance(collider, Target.collider) <= interactionRange)
        {
            ScriptableQuestOffer npcQuest = ((Npc)Target).quests[npcQuestIndex];
            if (npcQuest.completeHere)
            {
                int index = GetQuestIndexByName(npcQuest.quest.name);
                if (index != -1)
                {
                    // can complete it? (also checks inventory space for reward, if any)
                    Quest quest = quests[index];
                    if (CanCompleteQuest(quest.name))
                    {
                        // call quest.OnCompleted to remove quest items from
                        // inventory, etc.
                        quest.OnCompleted(this);

                        // gain rewards
                        Money += quest.rewardGold;
                        Experience += quest.rewardExperience;
                        if (quest.rewardItem != null)
                            InventoryAdd(new Item(quest.rewardItem), 1);

                        // complete quest
                        quest.completed = true;
                        quests[index] = quest;
                    }
                }
            }
        }
    }

    // npc trading /////////////////////////////////////////////////////////////
    [Command]
    public void CmdNpcBuyItem(int index, int amount)
    {
        // validate: close enough, npc alive and valid index?
        // use collider point(s) to also work with big entities
        if (State == "IDLE" &&
            Target != null &&
            Target.Health > 0 &&
            Target is Npc &&
            Utils.ClosestDistance(collider, Target.collider) <= interactionRange &&
            0 <= index && index < ((Npc)Target).saleItems.Length)
        {
            // valid amount?
            Item npcItem = new Item(((Npc)Target).saleItems[index]);
            if (1 <= amount && amount <= npcItem.maxStack)
            {
                long price = npcItem.buyPrice * amount;

                // enough gold and enough space in inventory?
                if (Money >= price && InventoryCanAdd(npcItem, amount))
                {
                    // pay for it, add to inventory
                    Money -= price;
                    InventoryAdd(npcItem, amount);
                }
            }
        }
    }

    [Command]
    public void CmdNpcSellItem(int index, int amount)
    {
        // validate: close enough, npc alive and valid index and valid item?
        // use collider point(s) to also work with big entities
        if (State == "IDLE" &&
            Target != null &&
            Target.Health > 0 &&
            Target is Npc &&
            Utils.ClosestDistance(collider, Target.collider) <= interactionRange &&
            0 <= index && index < inventory.Count)
        {
            // sellable?
            ItemSlot slot = inventory[index];
            if (slot.amount > 0 && slot.item.sellable && !slot.item.summoned)
            {
                // valid amount?
                if (1 <= amount && amount <= slot.amount)
                {
                    // sell the amount
                    long price = slot.item.sellPrice * amount;
                    Money += price;
                    slot.DecreaseAmount(amount);
                    inventory[index] = slot;
                }
            }
        }
    }

    // npc teleport ////////////////////////////////////////////////////////////
    [Command]
    public void CmdNpcTeleport()
    {
        // validate
        if (State == "IDLE" &&
            Target != null &&
            Target.Health > 0 &&
            Target is Npc &&
            Utils.ClosestDistance(collider, Target.collider) <= interactionRange &&
            ((Npc)Target).teleportTo != null)
        {
            // using agent.Warp is recommended over transform.position
            // (the latter can cause weird bugs when using it with an agent)
            agent.Warp(((Npc)Target).teleportTo.position);

            // clear target. no reason to keep targeting the npc after we
            // teleported away from it
            Target = null;
        }
    }

    // player to player trading ////////////////////////////////////////////////
    // how trading works:
    // 1. A invites his target with CmdTradeRequest()
    //    -> sets B.tradeInvitationFrom = A;
    // 2. B sees a UI window and accepts (= invites A too)
    //    -> sets A.tradeInvitationFrom = B;
    // 3. the TradeStart event is fired, both go to 'TRADING' state
    // 4. they lock the trades
    // 5. they accept, then items and gold are swapped

    public bool CanStartTrade()
    {
        // a player can only trade if he is not trading already and alive
        return Health > 0 && State != "TRADING";
    }

    public bool CanStartTradeWith(Entity entity)
    {
        // can we trade? can the target trade? are we close enough?
        return entity != null && entity is Player && entity != this &&
               CanStartTrade() && ((Player)entity).CanStartTrade() &&
               Utils.ClosestDistance(collider, entity.collider) <= interactionRange;
    }

    // request a trade with the target player.
    [Command]
    public void CmdTradeRequestSend()
    {
        // validate
        if (CanStartTradeWith(Target))
        {
            // send a trade request to target
            ((Player)Target).tradeRequestFrom = name;
            print(name + " invited " + Target.name + " to trade");
        }
    }

    // helper function to find the guy who sent us a trade invitation
    [Server]
    Player FindPlayerFromTradeInvitation()
    {
        if (tradeRequestFrom != "" && onlinePlayers.ContainsKey(tradeRequestFrom))
            return onlinePlayers[tradeRequestFrom];
        return null;
    }

    // accept a trade invitation by simply setting 'requestFrom' for the other
    // person to self
    [Command]
    public void CmdTradeRequestAccept()
    {
        Player sender = FindPlayerFromTradeInvitation();
        if (sender != null)
        {
            if (CanStartTradeWith(sender))
            {
                // also send a trade request to the person that invited us
                sender.tradeRequestFrom = name;
                print(name + " accepted " + sender.name + "'s trade request");
            }
        }
    }

    // decline a trade invitation
    [Command]
    public void CmdTradeRequestDecline()
    {
        tradeRequestFrom = "";
    }

    [Server]
    void TradeCleanup()
    {
        // clear all trade related properties
        tradeOfferGold = 0;
        for (int i = 0; i < tradeOfferItems.Count; ++i) tradeOfferItems[i] = -1;
        tradeStatus = TradeStatus.Free;
        tradeRequestFrom = "";
    }

    [Command]
    public void CmdTradeCancel()
    {
        // validate
        if (State == "TRADING")
        {
            // clear trade request for both guys. the FSM event will do the rest
            Player player = FindPlayerFromTradeInvitation();
            if (player != null) player.tradeRequestFrom = "";
            tradeRequestFrom = "";
        }
    }

    [Command]
    public void CmdTradeOfferLock()
    {
        // validate
        if (State == "TRADING")
            tradeStatus = TradeStatus.Locked;
    }

    [Command]
    public void CmdTradeOfferGold(long amount)
    {
        // validate
        if (State == "TRADING" && tradeStatus == TradeStatus.Free &&
            0 <= amount && amount <= Money)
            tradeOfferGold = amount;
    }

    [Command]
    public void CmdTradeOfferItem(int inventoryIndex, int offerIndex)
    {
        // validate
        if (State == "TRADING" && tradeStatus == TradeStatus.Free &&
            0 <= offerIndex && offerIndex < tradeOfferItems.Count &&
            !tradeOfferItems.Contains(inventoryIndex) && // only one reference
            0 <= inventoryIndex && inventoryIndex < inventory.Count)
        {
            ItemSlot slot = inventory[inventoryIndex];
            if (slot.amount > 0 && slot.item.tradable && !slot.item.summoned)
                tradeOfferItems[offerIndex] = inventoryIndex;
        }
    }

    [Command]
    public void CmdTradeOfferItemClear(int offerIndex)
    {
        // validate
        if (State == "TRADING" && tradeStatus == TradeStatus.Free &&
            0 <= offerIndex && offerIndex < tradeOfferItems.Count)
            tradeOfferItems[offerIndex] = -1;
    }

    [Server]
    bool IsTradeOfferStillValid()
    {
        // enough gold and all offered items are -1 or valid?
        return Money >= tradeOfferGold &&
               tradeOfferItems.All(index => index == -1 ||
                                            (0 <= index && index < inventory.Count && inventory[index].amount > 0));
    }

    [Server]
    int TradeOfferItemSlotAmount()
    {
        return tradeOfferItems.Count(i => i != -1);
    }

    [Server]
    int InventorySlotsNeededForTrade()
    {
        // if other guy offers 2 items and we offer 1 item then we only need
        // 2-1 = 1 slots. and the other guy would need 1-2 slots and at least 0.
        if (Target != null && Target is Player)
        {
            Player other = (Player)Target;
            int otherAmount = other.TradeOfferItemSlotAmount();
            int myAmount = TradeOfferItemSlotAmount();
            return Mathf.Max(otherAmount - myAmount, 0);
        }
        return 0;
    }

    [Command]
    public void CmdTradeOfferAccept()
    {
        // validate
        // note: distance check already done when starting the trade
        if (State == "TRADING" && tradeStatus == TradeStatus.Locked &&
            Target != null && Target is Player)
        {
            Player other = (Player)Target;

            // other has locked?
            if (other.tradeStatus == TradeStatus.Locked)
            {
                //  simply accept and wait for the other guy to accept too
                tradeStatus = TradeStatus.Accepted;
                print("first accept by " + name);
            }
            // other has accepted already? then both accepted now, start trade.
            else if (other.tradeStatus == TradeStatus.Accepted)
            {
                // accept
                tradeStatus = TradeStatus.Accepted;
                print("second accept by " + name);

                // both offers still valid?
                if (IsTradeOfferStillValid() && other.IsTradeOfferStillValid())
                {
                    // both have enough inventory slots?
                    // note: we don't use InventoryCanAdd here because:
                    // - current solution works if both have full inventories
                    // - InventoryCanAdd only checks one slot. here we have
                    //   multiple slots though (it could happen that we can
                    //   not add slot 2 after we did add slot 1's items etc)
                    if (InventorySlotsFree() >= InventorySlotsNeededForTrade() &&
                        other.InventorySlotsFree() >= other.InventorySlotsNeededForTrade())
                    {
                        // exchange the items by first taking them out
                        // into a temporary list and then putting them
                        // in. this guarantees that exchanging even
                        // works with full inventories

                        // take them out
                        Queue<ItemSlot> tempMy = new Queue<ItemSlot>();
                        foreach (int index in tradeOfferItems)
                        {
                            if (index != -1)
                            {
                                ItemSlot slot = inventory[index];
                                tempMy.Enqueue(slot);
                                slot.amount = 0;
                                inventory[index] = slot;
                            }
                        }

                        Queue<ItemSlot> tempOther = new Queue<ItemSlot>();
                        foreach (int index in other.tradeOfferItems)
                        {
                            if (index != -1)
                            {
                                ItemSlot slot = other.inventory[index];
                                tempOther.Enqueue(slot);
                                slot.amount = 0;
                                other.inventory[index] = slot;
                            }
                        }

                        // put them into the free slots
                        for (int i = 0; i < inventory.Count; ++i)
                            if (inventory[i].amount == 0 && tempOther.Count > 0)
                                inventory[i] = tempOther.Dequeue();

                        for (int i = 0; i < other.inventory.Count; ++i)
                            if (other.inventory[i].amount == 0 && tempMy.Count > 0)
                                other.inventory[i] = tempMy.Dequeue();

                        // did it all work?
                        if (tempMy.Count > 0 || tempOther.Count > 0)
                            Debug.LogWarning("item trade problem");

                        // exchange the gold
                        Money -= tradeOfferGold;
                        other.Money -= other.tradeOfferGold;

                        Money += other.tradeOfferGold;
                        other.Money += tradeOfferGold;
                    }
                }
                else print("trade canceled (invalid offer)");

                // clear trade request for both guys. the FSM event will do the
                // rest
                tradeRequestFrom = "";
                other.tradeRequestFrom = "";
            }
        }
    }

    // crafting ////////////////////////////////////////////////////////////////
    // the crafting system is designed to work with all kinds of commonly known
    // crafting options:
    // - item combinations: wood + stone = axe
    // - weapon upgrading: axe + gem = strong axe
    // - recipe items: axerecipe(item) + wood(item) + stone(item) = axe(item)
    //
    // players can craft at all times, not just at npcs, because that's the most
    // realistic option

    // craft the current combination of items and put result into inventory
    [Command]
    public void CmdCraft(int[] indices)
    {
        // validate: between 1 and 6, all valid, no duplicates?
        // -> can be IDLE or MOVING (in which case we reset the movement)
        if ((State == "IDLE" || State == "MOVING") &&
            indices.Length == ScriptableRecipe.recipeSize)
        {
            // find valid indices that are not '-1' and make sure there are no
            // duplicates
            List<int> validIndices = indices.Where(index => 0 <= index && index < inventory.Count && inventory[index].amount > 0).ToList();
            if (validIndices.Count > 0 && !validIndices.HasDuplicates())
            {
                // build list of item templates from valid indices
                List<ItemSlot> items = validIndices.Select(index => inventory[index]).ToList();

                // find recipe
                ScriptableRecipe recipe = ScriptableRecipe.dict.Values.ToList().Find(r => r.CanCraftWith(items)); // good enough for now
                if (recipe != null && recipe.result != null)
                {
                    // enough space?
                    Item result = new Item(recipe.result);
                    if (InventoryCanAdd(result, 1))
                    {
                        // store the crafting indices on the server. no need for
                        // a SyncList and unnecessary broadcasting.
                        // we already have a 'craftingIndices' variable anyway.
                        craftingIndices = indices.ToList();

                        // start crafting
                        craftingRequested = true;
                        craftingTimeEnd = NetworkTime.time + recipe.craftingTime;
                    }
                }
            }
        }
    }

    // finish the crafting
    [Server]
    void Craft()
    {
        // should only be called while CRAFTING
        // -> we already validated everything in CmdCraft. let's just craft.
        if (State == "CRAFTING")
        {
            // build list of item templates from indices
            List<int> validIndices = craftingIndices.Where(index => 0 <= index && index < inventory.Count && inventory[index].amount > 0).ToList();
            List<ItemSlot> items = validIndices.Select(index => inventory[index]).ToList();

            // find recipe
            ScriptableRecipe recipe = ScriptableRecipe.dict.Values.ToList().Find(r => r.CanCraftWith(items)); // good enough for now
            if (recipe != null && recipe.result != null)
            {
                // enough space?
                Item result = new Item(recipe.result);
                if (InventoryCanAdd(result, 1))
                {
                    // remove the ingredients from inventory in any case
                    foreach (ScriptableItemAndAmount ingredient in recipe.ingredients)
                        if (ingredient.amount > 0 && ingredient.item != null)
                            InventoryRemove(new Item(ingredient.item), ingredient.amount);

                    // roll the dice to decide if we add the result or not
                    // IMPORTANT: we use rand() < probability to decide.
                    // => UnityEngine.Random.value is [0,1] inclusive:
                    //    for 0% probability it's fine because it's never '< 0'
                    //    for 100% probability it's not because it's not always '< 1', it might be == 1
                    //    and if we use '<=' instead then it won't work for 0%
                    // => C#'s Random value is [0,1) exclusive like most random
                    //    functions. this works fine.
                    if (new System.Random().NextDouble() < recipe.probability)
                    {
                        // add result item to inventory
                        InventoryAdd(new Item(recipe.result), 1);
                        TargetCraftingSuccess();
                    }
                    else
                    {
                        TargetCraftingFailed();
                    }

                    // clear indices afterwards
                    // note: we set all to -1 instead of calling .Clear because
                    //       that would clear all the slots in host mode.
                    for (int i = 0; i < ScriptableRecipe.recipeSize; ++i)
                        craftingIndices[i] = -1;
                }
            }
        }
    }

    // two rpcs for results to save 1 byte for the actual result
    [TargetRpc]
    public void TargetCraftingSuccess()
    {
        craftingState = CraftingState.Success;
    }

    [TargetRpc]
    public void TargetCraftingFailed()
    {
        craftingState = CraftingState.Failed;
    }

    // pvp murder system ///////////////////////////////////////////////////////
    // attacking someone innocent results in Offender status
    //   (can be attacked without penalty for a short time)
    // killing someone innocent results in Murderer status
    //   (can be attacked without penalty for a long time + negative buffs)
    // attacking/killing a Offender/Murderer has no penalty
    //
    // we use buffs for the offender/status because buffs have all the features
    // that we need here.
    public bool IsOffender()
    {
        return offenderBuff != null && buffs.Any(buff => buff.name == offenderBuff.name);
    }

    public bool IsMurderer()
    {
        return murdererBuff != null && buffs.Any(buff => buff.name == murdererBuff.name);
    }

    public void StartOffender()
    {
        if (offenderBuff != null) AddOrRefreshBuff(new Buff(offenderBuff, 1));
    }

    public void StartMurderer()
    {
        if (murdererBuff != null) AddOrRefreshBuff(new Buff(murdererBuff, 1));
    }

    // item mall ///////////////////////////////////////////////////////////////
    [Command]
    public void CmdEnterCoupon(string coupon)
    {
        // only allow entering one coupon every few seconds to avoid brute force
        if (NetworkTime.time >= nextRiskyActionTime)
        {
            // YOUR COUPON VALIDATION CODE HERE
            // coins += ParseCoupon(coupon);
            Debug.Log("coupon: " + coupon + " => " + name + "@" + NetworkTime.time);
            nextRiskyActionTime = NetworkTime.time + couponWaitSeconds;
        }
    }

    [Command]
    public void CmdUnlockItem(int categoryIndex, int itemIndex)
    {
        // validate: only if alive so people can't buy resurrection potions
        // after dieing in a PvP fight etc.
        if (Health > 0 &&
            0 <= categoryIndex && categoryIndex <= itemMallCategories.Length &&
            0 <= itemIndex && itemIndex <= itemMallCategories[categoryIndex].items.Length)
        {
            Item item = new Item(itemMallCategories[categoryIndex].items[itemIndex]);
            if (0 < item.itemMallPrice && item.itemMallPrice <= coins)
            {
                // try to add it to the inventory, subtract costs from coins
                if (InventoryAdd(item, 1))
                {
                    coins -= item.itemMallPrice;
                    Debug.Log(name + " unlocked " + item.name);

                    // NOTE: item mall purchases need to be persistent, yet
                    // resaving the player here is not necessary because if the
                    // server crashes before next save, then both the inventory
                    // and the coins will be reverted anyway.
                }
            }
        }
    }

    // coins can't be increased by an external application while the player is
    // ingame. we use an additional table to store new orders in and process
    // them every few seconds from here. this way we can even notify the player
    // after his order was processed successfully.
    //
    // note: the alternative is to keep player.coins in the database at all
    // times, but then we need RPCs and the client needs a .coins value anyway.
    [Server]
    void ProcessCoinOrders()
    {
        List<long> orders = Database.singleton.GrabCharacterOrders(name);
        foreach (long reward in orders)
        {
            coins += reward;
            Debug.Log("Processed order for: " + name + ";" + reward);
            string message = "Processed order for: " + reward;
            chat.TargetMsgInfo(message);
        }
    }

    // guild ///////////////////////////////////////////////////////////////////
    public bool InGuild()
    {
        return !string.IsNullOrWhiteSpace(guild.name);
    }

    [Server]
    public void SetGuildOnline(bool online)
    {
        // validate
        if (InGuild())
            GuildSystem.SetGuildOnline(guild.name, name, online);
    }

    [Command]
    public void CmdGuildInviteTarget()
    {
        // validate
        if (Target != null && Target is Player &&
            InGuild() && !((Player)Target).InGuild() &&
            guild.CanInvite(name, Target.name) &&
            NetworkTime.time >= nextRiskyActionTime &&
            Utils.ClosestDistance(collider, Target.collider) <= interactionRange)
        {
            // send a invite and reset risky time
            ((Player)Target).guildInviteFrom = name;
            nextRiskyActionTime = NetworkTime.time + guildInviteWaitSeconds;
            print(name + " invited " + Target.name + " to guild");
        }
    }

    [Command]
    public void CmdGuildInviteAccept()
    {
        // valid invitation, sender exists and is in a guild?
        // note: no distance check because sender might be far away already
        if (!InGuild() && guildInviteFrom != "" &&
            onlinePlayers.TryGetValue(guildInviteFrom, out Player sender) &&
            sender.InGuild())
        {
            // try to add. GuildSystem does all the checks.
            GuildSystem.AddToGuild(sender.guild.name, sender.name, name, level);
        }

        // reset guild invite in any case
        guildInviteFrom = "";
    }

    [Command]
    public void CmdGuildInviteDecline()
    {
        guildInviteFrom = "";
    }

    [Command]
    public void CmdGuildKick(string memberName)
    {
        // validate
        if (InGuild())
            GuildSystem.KickFromGuild(guild.name, name, memberName);
    }

    [Command]
    public void CmdGuildPromote(string memberName)
    {
        // validate
        if (InGuild())
            GuildSystem.PromoteMember(guild.name, name, memberName);
    }

    [Command]
    public void CmdGuildDemote(string memberName)
    {
        // validate
        if (InGuild())
            GuildSystem.DemoteMember(guild.name, name, memberName);
    }

    [Command]
    public void CmdSetGuildNotice(string notice)
    {
        // validate
        // (only allow changes every few seconds to avoid bandwidth issues)
        if (InGuild() && NetworkTime.time >= nextRiskyActionTime)
        {
            // try to set notice. reset time if it worked.
            if (GuildSystem.SetGuildNotice(guild.name, name, notice))
                nextRiskyActionTime = NetworkTime.time + GuildSystem.NoticeWaitSeconds;
        }
    }

    // helper function to check if we are near a guild manager npc
    public bool IsGuildManagerNear()
    {
        return Target != null &&
               Target is Npc &&
               ((Npc)Target).offersGuildManagement &&
               Utils.ClosestDistance(collider, Target.collider) <= interactionRange;
    }

    [Command]
    public void CmdTerminateGuild()
    {
        // validate
        if (InGuild() && IsGuildManagerNear())
            GuildSystem.TerminateGuild(guild.name, name);
    }

    [Command]
    public void CmdCreateGuild(string guildName)
    {
        // validate
        if (Health > 0 && Money >= GuildSystem.CreationPrice &&
            !InGuild() && IsGuildManagerNear())
        {
            // try to create the guild. pay for it if it worked.
            if (GuildSystem.CreateGuild(name, level, guildName))
                Money -= GuildSystem.CreationPrice;
            else
                chat.TargetMsgInfo("Guild name invalid!");
        }
    }

    [Command]
    public void CmdLeaveGuild()
    {
        // validate
        if (InGuild())
            GuildSystem.LeaveGuild(guild.name, name);
    }

    // party ///////////////////////////////////////////////////////////////////
    public bool InParty()
    {
        // 0 means no party, because default party struct's partyId is 0.
        return party.partyId > 0;
    }

    // find party members in proximity for item/exp sharing etc.
    public List<Player> GetPartyMembersInProximity()
    {
        if (InParty())
        {
            return netIdentity.observers.Values
                                        .Select(conn => conn.playerController.GetComponent<Player>())
                                        .Where(p => party.Contains(p.name))
                                        .ToList();
        }
        return new List<Player>();
    }

    // party invite by name (not by target) so that chat commands are possible
    // if needed
    [Command]
    public void CmdPartyInvite(string otherName)
    {
        // validate: is there someone with that name, and not self?
        if (otherName != name && onlinePlayers.ContainsKey(otherName) &&
            NetworkTime.time >= nextRiskyActionTime)
        {
            Player other = onlinePlayers[otherName];

            // can only send invite if no party yet or party isn't full and
            // have invite rights and other guy isn't in party yet
            if ((!InParty() || !party.IsFull()) && !other.InParty())
            {
                // send a invite and reset risky time
                other.partyInviteFrom = name;
                nextRiskyActionTime = NetworkTime.time + partyInviteWaitSeconds;
                print(name + " invited " + other.name + " to party");
            }
        }
    }

    [Command]
    public void CmdPartyInviteAccept()
    {
        // valid invitation?
        // note: no distance check because sender might be far away already
        if (!InParty() && partyInviteFrom != "" &&
            onlinePlayers.ContainsKey(partyInviteFrom))
        {
            // find sender
            Player sender = onlinePlayers[partyInviteFrom];

            // is in party? then try to add
            if (sender.InParty())
                PartySystem.AddToParty(sender.party.partyId, name);
            // otherwise try to form a new one
            else
                PartySystem.FormParty(sender.name, name);
        }

        // reset party invite in any case
        partyInviteFrom = "";
    }

    [Command]
    public void CmdPartyInviteDecline()
    {
        partyInviteFrom = "";
    }

    [Command]
    public void CmdPartyKick(string member)
    {
        // try to kick. party system will do all the validation.
        PartySystem.KickFromParty(party.partyId, name, member);
    }

    // version without cmd because we need to call it from the server too
    public void PartyLeave()
    {
        // try to leave. party system will do all the validation.
        PartySystem.LeaveParty(party.partyId, name);
    }
    [Command]
    public void CmdPartyLeave() { PartyLeave(); }

    // version without cmd because we need to call it from the server too
    public void PartyDismiss()
    {
        // try to dismiss. party system will do all the validation.
        PartySystem.DismissParty(party.partyId, name);
    }
    [Command]
    public void CmdPartyDismiss() { PartyDismiss(); }

    [Command]
    public void CmdPartySetExperienceShare(bool value)
    {
        // try to set. party system will do all the validation.
        PartySystem.SetPartyExperienceShare(party.partyId, name, value);
    }

    [Command]
    public void CmdPartySetGoldShare(bool value)
    {
        // try to set. party system will do all the validation.
        PartySystem.SetPartyGoldShare(party.partyId, name, value);
    }

    // pet /////////////////////////////////////////////////////////////////////
    [Command]
    public void CmdPetSetAutoAttack(bool value)
    {
        // validate
        if (ActivePet != null)
            ActivePet.autoAttack = value;
    }

    [Command]
    public void CmdPetSetDefendOwner(bool value)
    {
        // validate
        if (ActivePet != null)
            ActivePet.defendOwner = value;
    }

    // helper function for command and UI
    public bool CanUnsummonPet()
    {
        // only while pet and owner aren't fighting
        return ActivePet != null &&
               (State == "IDLE" || State == "MOVING") &&
               (ActivePet.State == "IDLE" || ActivePet.State == "MOVING");
    }

    [Command]
    public void CmdPetUnsummon()
    {
        // validate
        if (CanUnsummonPet())
        {
            // destroy from world. item.summoned and activePet will be null.
            NetworkServer.Destroy(ActivePet.gameObject);
        }
    }

    [Command]
    public void CmdNpcReviveSummonable(int index)
    {
        // validate: close enough, npc alive and valid index and valid item?
        // use collider point(s) to also work with big entities
        if (State == "IDLE" &&
            Target != null &&
            Target.Health > 0 &&
            Target is Npc &&
            ((Npc)Target).offersSummonableRevive &&
            Utils.ClosestDistance(collider, Target.collider) <= interactionRange &&
            0 <= index && index < inventory.Count)
        {
            ItemSlot slot = inventory[index];
            if (slot.amount > 0 && slot.item.data is SummonableItem)
            {
                // verify the pet status
                SummonableItem itemData = (SummonableItem)slot.item.data;
                if (slot.item.summonedHealth == 0 && itemData.summonPrefab != null)
                {
                    // enough gold?
                    if (Money >= itemData.revivePrice)
                    {
                        // pay for it, revive it
                        Money -= itemData.revivePrice;
                        slot.item.summonedHealth = itemData.summonPrefab.HealthMax;
                        inventory[index] = slot;
                    }
                }
            }
        }
    }

    // mounts //////////////////////////////////////////////////////////////////
    public bool IsMounted()
    {
        return ActiveMount != null && ActiveMount.Health > 0;
    }

    // selection handling //////////////////////////////////////////////////////
    public void SetIndicatorViaParent(Transform parent)
    {
        if (!indicator) indicator = Instantiate(indicatorPrefab);
        indicator.transform.SetParent(parent, true);
        indicator.transform.position = parent.position;
    }

    public void SetIndicatorViaPosition(Vector2 pos)
    {
        if (!indicator) indicator = Instantiate(indicatorPrefab);
        indicator.transform.parent = null;
        indicator.transform.position = pos;
    }

    [Command]
    public void CmdSetTarget(NetworkIdentity ni)
    {
        // validate
        if (ni != null)
        {
            // can directly change it, or change it after casting?
            if (State == "IDLE" || State == "MOVING" || State == "STUNNED")
                Target = ni.GetComponent<Entity>();
            else if (State == "CASTING")
                NextTarget = ni.GetComponent<Entity>();
        }
    }

    [Client]
    void SelectionHandling()
    {
        // click raycasting if not over a UI element & not pinching on mobile
        // note: this only works if the UI's CanvasGroup blocks Raycasts
        if (Input.GetMouseButtonDown(0) && !Utils.IsCursorOverUserInterface() && Input.touchCount <= 1)
        {
            // cast a 3D ray from the camera towards the 2D scene.
            // Physics2D.Raycast isn't made for that, we use GetRayIntersection.
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // raycast with local player ignore option
            RaycastHit2D hit = localPlayerClickThrough ? Utils.Raycast2DWithout(ray, gameObject) : Physics2D.GetRayIntersection(ray);

            // clear requested skill in any case because if we clicked
            // somewhere else then we don't care about it anymore
            useSkillWhenCloser = -1;

            // valid target?
            Entity entity = hit.transform != null ? hit.transform.GetComponent<Entity>() : null;
            if (entity)
            {
                // set indicator
                SetIndicatorViaParent(hit.transform);

                // clicked last target again? and is not self or pet?
                if (entity == Target && entity != this && entity != ActivePet)
                {
                    // attackable and has skills? => attack
                    if (CanAttack(entity) && skills.Count > 0)
                    {
                        // then try to use that one
                        TryUseSkill(0);
                    }
                    // npc, alive, close enough? => talk
                    // use collider point(s) to also work with big entities
                    else if (entity is Npc && entity.Health > 0 && Utils.ClosestDistance(collider, entity.collider) <= interactionRange)
                    {
                        UINpcDialogue.singleton.Show();
                    }
                    // monster, dead, has loot, close enough? => loot
                    // use collider point(s) to also work with big entities
                    else if (entity is Monster && entity.Health == 0 && Utils.ClosestDistance(collider, entity.collider) <= interactionRange && ((Monster)entity).HasLoot())
                    {
                        UILoot.singleton.Show();
                    }
                    else if (entity is ItemEntity && entity.Health >= 0 && Utils.ClosestDistance(collider, entity.collider) <= interactionRange)
                    {
                        InventoryAdd(new Item(((ItemEntity)entity).ItemInfo), 1);
                        Debug.Log("拾取了一个：" + entity.name);
                        Destroy(entity.gameObject);

                        agent.stoppingDistance = interactionRange;
                        agent.destination = entity.collider.ClosestPointOnBounds(transform.position);
                    }
                    // not attackable, lootable, talkable, etc., but it's
                    // still an entity and double clicking it without doing
                    // anything would be strange.
                    // (e.g. if we are in a safe zone and click on a
                    //  monster. it's not attackable, but we should at least
                    //  move there, otherwise double click feels broken)
                    else
                    {
                        // use collider point(s) to also work with big entities
                        agent.stoppingDistance = interactionRange;
                        agent.destination = entity.collider.ClosestPointOnBounds(transform.position);
                    }

                    // addon system hooks
                    Utils.InvokeMany(typeof(Player), this, "OnSelect_", entity);
                    // clicked a new target
                }
                else
                {
                    // target it
                    CmdSetTarget(entity.netIdentity);
                }
            }
            // if we hit nothing then we want to move somewhere
            else
            {
                // set indicator and navigate to the nearest walkable
                // destination. this prevents twitching when destination is
                // accidentally in a room without a door etc.
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 bestDestination = agent.NearestValidDestination(worldPos);
                SetIndicatorViaPosition(bestDestination);

                // casting? then set pending destination
                if (State == "CASTING")
                {
                    pendingDestination = bestDestination;
                    pendingDestinationValid = true;
                }
                else
                {
                    agent.stoppingDistance = 0;
                    agent.destination = bestDestination;
                }
            }
        }
        //如果点击鼠标右键
        if (Input.GetMouseButtonDown(1) && !Utils.IsCursorOverUserInterface() && Input.touchCount <= 1)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = localPlayerClickThrough ? Utils.Raycast2DWithout(ray, gameObject) : Physics2D.GetRayIntersection(ray);
            useSkillWhenCloser = -1;

            Entity entity = hit.transform != null ? hit.transform.GetComponent<Entity>() : null;
            //如果点中实体
            if (entity)
            {
                if (entity == Target && entity != this && entity != ActivePet)
                {
                    if (entity is Npc && entity.Health > 0 && Utils.ClosestDistance(collider, entity.collider) <= interactionRange)
                    {
                        //UINpcDialogue.singleton.Show();
                    }
                    else if (entity is Monster && entity.Health == 0 && Utils.ClosestDistance(collider, entity.collider) <= interactionRange && ((Monster)entity).HasLoot())
                    {
                        //UILoot.singleton.Show();
                    }
                    else if (entity is ItemEntity && entity.Health >= 0 && Utils.ClosestDistance(collider, entity.collider) <= interactionRange)
                    {
                        //Debug.Log("右键点击：" + entity.name);
                    }
                }
                else
                {
                    CmdSetTarget(entity.netIdentity);
                }
            }
        }
    }

    [Client]
    [Obsolete]
    void WASDHandling()
    {
        // don't move if currently typing in an input
        // we check this after checking h and v to save computations
        if (!UIUtils.AnyInputActive())
        {
            // get horizontal and vertical input
            // note: no != 0 check because it's 0 when we stop moving rapidly
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (horizontal != 0 || vertical != 0)
            {
                // create direction, normalize in case of diagonal movement
                Vector2 direction = new Vector2(horizontal, vertical);
                if (direction.magnitude > 1) direction = direction.normalized;

                // draw direction for debugging
                Debug.DrawLine(transform.position, transform.position + (Vector3)direction, Color.green, 0, false);

                // clear indicator if there is one, and if it's not on a target
                // (simply looks better)
                if (direction != Vector2.zero && indicator != null && indicator.transform.parent == null)
                    Destroy(indicator);

                // cancel path if we are already doing click movement, otherwise
                // we will slide
                agent.ResetMovement();

                //是否跑步
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    currentSpeed = Speed * RunSpeedMultiple;
                }
                else
                {
                    currentSpeed = Speed;
                }
                // casting? then set pending velocity
                if (State == "CASTING")
                {
                    pendingVelocity = direction * currentSpeed;
                    pendingVelocityValid = true;
                }
                else
                {
                    agent.velocity = direction * currentSpeed;
                }

                // clear requested skill in any case because if we clicked
                // somewhere else then we don't care about it anymore
                useSkillWhenCloser = -1;
            }
        }
    }

    // simple tab targeting
    [Client]
    void TargetNearest()
    {
        if (Input.GetKeyDown(targetNearestKey))
        {
            // find all monsters that are alive, sort by distance
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Monster");
            List<Monster> monsters = objects.Select(go => go.GetComponent<Monster>()).Where(m => m.Health > 0).ToList();
            List<Monster> sorted = monsters.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();

            // target nearest one
            if (sorted.Count > 0)
            {
                SetIndicatorViaParent(sorted[0].transform);
                CmdSetTarget(sorted[0].netIdentity);
            }
        }
    }

    // ontrigger ///////////////////////////////////////////////////////////////
    protected override void OnTriggerEnter2D(Collider2D col)
    {
        // call base function too
        base.OnTriggerEnter2D(col);

        // quest location?
        if (isServer && col.tag == "QuestLocation")
            QuestsOnLocation(col);
    }

    // drag and drop ///////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_InventorySlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

        // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
        if (inventory[slotIndices[0]].amount > 0 && inventory[slotIndices[1]].amount > 0 &&
            inventory[slotIndices[0]].item.Equals(inventory[slotIndices[1]].item))
        {
            CmdInventoryMerge(slotIndices[0], slotIndices[1]);
        }
        // split?
        else if (Utils.AnyKeyPressed(inventorySplitKeys))
        {
            CmdInventorySplit(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            CmdSwapInventoryInventory(slotIndices[0], slotIndices[1]);
        }
    }

    void OnDragAndDrop_InventorySlot_TrashSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        CmdSwapInventoryTrash(slotIndices[0]);
    }

    void OnDragAndDrop_InventorySlot_EquipmentSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        CmdSwapInventoryEquip(slotIndices[0], slotIndices[1]);
    }

    void OnDragAndDrop_InventorySlot_SkillbarSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        skillbar[slotIndices[1]].reference = inventory[slotIndices[0]].item.name; // just save it clientsided
    }

    void OnDragAndDrop_InventorySlot_NpcSellSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        ItemSlot slot = inventory[slotIndices[0]];
        if (slot.item.sellable && !slot.item.summoned)
        {
            UINpcTrading.singleton.sellIndex = slotIndices[0];
            UINpcTrading.singleton.sellAmountInput.text = slot.amount.ToString();
        }
    }

    void OnDragAndDrop_InventorySlot_TradingSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (inventory[slotIndices[0]].item.tradable)
            CmdTradeOfferItem(slotIndices[0], slotIndices[1]);
    }

    void OnDragAndDrop_InventorySlot_CraftingIngredientSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // only if not crafting right now
        if (craftingState != CraftingState.InProgress)
        {
            if (!craftingIndices.Contains(slotIndices[0]))
            {
                craftingIndices[slotIndices[1]] = slotIndices[0];
                craftingState = CraftingState.None; // reset state
            }
        }
    }

    void OnDragAndDrop_TrashSlot_InventorySlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        CmdSwapTrashInventory(slotIndices[1]);
    }

    void OnDragAndDrop_EquipmentSlot_InventorySlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        CmdSwapInventoryEquip(slotIndices[1], slotIndices[0]); // reversed
    }

    void OnDragAndDrop_EquipmentSlot_SkillbarSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        skillbar[slotIndices[1]].reference = equipment[slotIndices[0]].item.name; // just save it clientsided
    }

    void OnDragAndDrop_SkillsSlot_SkillbarSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        skillbar[slotIndices[1]].reference = skills[slotIndices[0]].name; // just save it clientsided
    }

    void OnDragAndDrop_SkillbarSlot_SkillbarSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // just swap them clientsided
        string temp = skillbar[slotIndices[0]].reference;
        skillbar[slotIndices[0]].reference = skillbar[slotIndices[1]].reference;
        skillbar[slotIndices[1]].reference = temp;
    }

    void OnDragAndDrop_CraftingIngredientSlot_CraftingIngredientSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // only if not crafting right now
        if (craftingState != CraftingState.InProgress)
        {
            // just swap them clientsided
            int temp = craftingIndices[slotIndices[0]];
            craftingIndices[slotIndices[0]] = craftingIndices[slotIndices[1]];
            craftingIndices[slotIndices[1]] = temp;
            craftingState = CraftingState.None; // reset state
        }
    }

    void OnDragAndDrop_InventorySlot_NpcReviveSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (inventory[slotIndices[0]].item.data is PetItem)
            UINpcRevive.singleton.itemIndex = slotIndices[0];
    }

    void OnDragAndClear_SkillbarSlot(int slotIndex)
    {
        skillbar[slotIndex].reference = "";
    }

    void OnDragAndClear_TradingSlot(int slotIndex)
    {
        CmdTradeOfferItemClear(slotIndex);
    }

    void OnDragAndClear_NpcSellSlot(int slotIndex)
    {
        UINpcTrading.singleton.sellIndex = -1;
    }

    void OnDragAndClear_CraftingIngredientSlot(int slotIndex)
    {
        // only if not crafting right now
        if (craftingState != CraftingState.InProgress)
        {
            craftingIndices[slotIndex] = -1;
            craftingState = CraftingState.None; // reset state
        }
    }

    void OnDragAndClear_NpcReviveSlot(int slotIndex)
    {
        UINpcRevive.singleton.itemIndex = -1;
    }

    // validation //////////////////////////////////////////////////////////////
    void OnValidate()
    {
        // make sure that the NetworkNavMeshAgentRubberbanding2D component is
        // ABOVE the player component, so that it gets updated before Player.cs.
        // -> otherwise it overwrites player's WASD velocity for local player
        //    hosts
        // -> there might be away around it, but a warning is good for now
        Component[] components = GetComponents<Component>();
        if (Array.IndexOf(components, GetComponent<NetworkNavMeshAgentRubberbanding2D>()) >
            Array.IndexOf(components, this))
            Debug.LogWarning(name + "'s NetworkNavMeshAgentRubberbanding2D component is below the Player component. Please drag it above the Player component in the Inspector, otherwise there might be WASD movement issues due to the Update order.");
    }
}
