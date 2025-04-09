using UnityEngine;
using UnityEngine.UI;

namespace TestProject.UI
{
    [RequireComponent(typeof(Image))]
    public class LoaderImage : MonoBehaviour
    {
        private void Update()
        {
            transform.Rotate(360f * Time.deltaTime * Vector3.forward);
        }
    }
}
