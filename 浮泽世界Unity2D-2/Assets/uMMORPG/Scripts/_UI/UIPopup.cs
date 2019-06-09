using UnityEngine;
using UnityEngine.UI;

namespace E.Game
{
    public class UIPopup : UIBase
    {
        public static UIPopup singleton;
        public Text messageText;

        public UIPopup()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            if (singleton == null) singleton = this;
        }

        public void Show(string message)
        {
            // append error if visible, set otherwise. then show it.
            if (panel.activeSelf) messageText.text += ";\n" + message;
            else messageText.text = message;
            panel.SetActive(true);
        }
    }
}