// Copy text from one text mesh to another (for shadows etc.)
using UnityEngine;

namespace E.Utility
{
    [RequireComponent(typeof(TextMesh))]
    public class TextMeshCopyText : MonoBehaviour
    {
        public TextMesh source;
        public TextMesh destination;

        void Update()
        {
            destination.text = source.text;
        }
    }
}