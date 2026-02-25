using UnityEngine;

namespace Project.Core
{
    public static class UIHelper
    {
        /// <summary>
        /// Destroys all child GameObjects under the given parent Transform.
        /// </summary>
        public static void DestroyAllChildren(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}
