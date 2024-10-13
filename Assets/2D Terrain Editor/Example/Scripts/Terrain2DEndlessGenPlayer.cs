using UnityEngine;

namespace T2D.Example
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Terrain2DEndlessGenPlayer : MonoBehaviour
    {
        public Rigidbody2D Rigidbody => _rigidbody ?? (_rigidbody = GetComponent<Rigidbody2D>());
        private Rigidbody2D _rigidbody;
        
        
        public float MaxSpeed = 1f;


        private bool _isUserInput;


        void Update()
        {
            //Check if the user has pressed any horizontal axis button
            if (!_isUserInput && !Mathf.Approximately(Input.GetAxis("Horizontal"), 0))
                _isUserInput = true;

            //Set constant velocity until _isUserInput is false
            float targetVelocity = _isUserInput ? -Input.GetAxis("Horizontal") * MaxSpeed : - MaxSpeed;

            //Apply angular velocity
            Rigidbody.angularVelocity = Mathf.Lerp(Rigidbody.angularVelocity, targetVelocity, Time.deltaTime * 5);
        }
    }
}