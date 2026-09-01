using UnityEngine;

namespace VADE.DevTools.UI
{

    public interface IWindowFactory
    {
        T Create<T>(Transform parent) where T : Window;
    }
}
