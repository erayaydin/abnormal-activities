using UnityEngine;

namespace Horde.Abnormal.Arch
{
    public abstract class SyncSingleton : MonoBehaviour
    {
        protected static readonly object Lock = new();
    }
}