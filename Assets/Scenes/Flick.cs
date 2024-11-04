/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Gestures;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace TouchScript.Examples.Flick
{
    /// <exclude />
    public class Flick : MonoBehaviour
    {
        private FlickGesture gesture;

        private void OnEnable()
        {
            gesture = GetComponent<FlickGesture>();
            gesture.Flicked += tappedHandler;
        }

        private void OnDisable()
        {
            gesture.Flicked -= tappedHandler;
        }

        private void tappedHandler(object sender, System.EventArgs e)
        {
            Vector3 position = transform.position;
            position.x -= 10;
            position.y += 5;
            position.z -= 10;
            transform.position = position;
        }
    }
}