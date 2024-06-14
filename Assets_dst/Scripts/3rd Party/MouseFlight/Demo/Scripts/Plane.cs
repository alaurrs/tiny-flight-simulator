//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;
using UnityEngine.UI;

namespace MFlight.Demo
{
    [RequireComponent(typeof(Rigidbody))]
    public class Plane : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private MouseFlightController controller = null;
        [SerializeField] private Slider throttleSlider = null; // Ajout de la référence au Slider

        [Header("Physics")]
        [Tooltip("Force to push plane forwards with")] public float thrust = 100f;
        [Tooltip("Pitch, Yaw, Roll")] public Vector3 turnTorque = new Vector3(90f, 25f, 45f);
        [Tooltip("Multiplier for all forces")] public float forceMult = 1000f;

        [Header("Autopilot")]
        [Tooltip("Sensitivity for autopilot flight.")] public float sensitivity = 5f;
        [Tooltip("Angle at which airplane banks fully into target.")] public float aggressiveTurnAngle = 10f;

        [Header("Input")]
        [SerializeField] [Range(-1f, 1f)] private float pitch = 0f;
        [SerializeField] [Range(-1f, 1f)] private float yaw = 0f;
        [SerializeField] [Range(-1f, 1f)] private float roll = 0f;

        public float Pitch
        {
            set { pitch = Mathf.Clamp(value, -1f, 1f); }
            get { return pitch; }
        }
        public float Yaw
        {
            set { yaw = Mathf.Clamp(value, -1f, 1f); }
            get { return yaw; }
        }
        public float Roll
        {
            set { roll = Mathf.Clamp(value, -1f, 1f); }
            get { return roll; }
        }

        private Rigidbody rigid;

        private bool rollOverride = false;
        private bool pitchOverride = false;

        [Header("Fuel Settings")]
        [SerializeField] private float fuel = 1000f;
        [SerializeField] private float fuelConsumptionRate = 1f;

        [Header("Throttle Settings")]
        [SerializeField] private float throttleChangeRate = 20f; // Ajuster la vitesse de changement du throttle
        [SerializeField] [Range(0f, 100f)] private float throttle = 50f; // Throttle de 0 à 100

        private float lastAltitude = 0f;
        private bool isOutOfFuel = false;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();

            if (controller == null)
                Debug.LogError(name + ": Plane - Missing reference to MouseFlightController!");

            if (throttleSlider == null)
                Debug.LogError(name + ": Plane - Missing reference to Throttle Slider!");
        }

        private void Update()
        {
            if (!isOutOfFuel)
            {
                rollOverride = false;
                pitchOverride = false;

                float keyboardRoll = Input.GetAxis("Horizontal");
                if (Mathf.Abs(keyboardRoll) > .25f)
                {
                    rollOverride = true;
                }

                float keyboardPitch = Input.GetAxis("Vertical");
                if (Mathf.Abs(keyboardPitch) > .25f)
                {
                    pitchOverride = true;
                    rollOverride = true;
                }

                float autoYaw = 0f;
                float autoPitch = 0f;
                float autoRoll = 0f;
                if (controller != null)
                    RunAutopilot(controller.MouseAimPos, out autoYaw, out autoPitch, out autoRoll);

                yaw = autoYaw;
                pitch = (pitchOverride) ? keyboardPitch : autoPitch;
                roll = (rollOverride) ? keyboardRoll : autoRoll;

                // Consommation de carburant en fonction du niveau des gaz
                fuel -= Time.deltaTime * fuelConsumptionRate * (throttle / 100f);
                if (fuel < 0f) fuel = 0f;

                // Mise à jour de l'altitude pour calculer la vitesse verticale
                float currentAltitude = GetAltitude();
                lastAltitude = currentAltitude;

                // Vérifier le niveau de carburant
                if (fuel == 0f)
                {
                    OnOutOfFuel();
                }

                // Gérer l'accélération et la décélération
                HandleThrottle();

                // Mettre à jour le Slider
                if (throttleSlider != null)
                {
                    throttleSlider.value = throttle;
                }
            }
        }

        // private void HandleThrottle()
        // {
        //     // Utilisation combinée du clavier et de la manette
        //     // float throttleUpInput = 0f;
        //     // float throttleDownInput = 0f;

        //     // if (Input.GetButton("ThrottleUp"))
        //     // {
        //     //     throttleUpInput = 1f;
        //     // }
        //     // if (Input.GetButton("ThrottleDown"))
        //     // {
        //     //     throttleDownInput = 1f;
        //     // }

        //     float throttleUpInput = Input.GetAxis("ThrottleUp");
        //     float throttleDownInput = Input.GetAxis("ThrottleDown");

        //     throttle += throttleChangeRate * throttleUpInput * Time.deltaTime;
        //     throttle -= throttleChangeRate * throttleDownInput * Time.deltaTime;

        //     throttle = Mathf.Clamp(throttle, 0f, 100f);
        // }
        private void HandleThrottle()
        {
            // Utilisation combinée du clavier et de la manette
            float throttleUpInput = 0f;
            float throttleDownInput = 0f;

            // Vérifier les entrées clavier
            if (Input.GetButton("ThrottleUp"))
            {
                throttleUpInput = 1f;
            }
            if (Input.GetButton("ThrottleDown"))
            {
                throttleDownInput = 1f;
            }

            // Vérifier les entrées manette
            throttleUpInput += Mathf.Clamp(Input.GetAxis("ThrottleUpController"), 0, 1);
            throttleDownInput += Mathf.Clamp(Input.GetAxis("ThrottleDownController"), 0, 1);

            // Ajuster les gaz en fonction des entrées
            throttle += throttleChangeRate * throttleUpInput * Time.deltaTime;
            throttle -= throttleChangeRate * throttleDownInput * Time.deltaTime;

            throttle = Mathf.Clamp(throttle, 0f, 100f);
        }




        private void RunAutopilot(Vector3 flyTarget, out float yaw, out float pitch, out float roll)
        {
            var localFlyTarget = transform.InverseTransformPoint(flyTarget).normalized * sensitivity;
            var angleOffTarget = Vector3.Angle(transform.forward, flyTarget - transform.position);

            yaw = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
            pitch = -Mathf.Clamp(localFlyTarget.y, -1f, 1f);

            var aggressiveRoll = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
            var wingsLevelRoll = transform.right.y;

            var wingsLevelInfluence = Mathf.InverseLerp(0f, aggressiveTurnAngle, angleOffTarget);
            roll = Mathf.Lerp(wingsLevelRoll, aggressiveRoll, wingsLevelInfluence);
        }

        private void FixedUpdate()
        {
            if (!isOutOfFuel)
            {
                rigid.AddRelativeForce(Vector3.forward * thrust * forceMult * (throttle / 100f), ForceMode.Force);
                rigid.AddRelativeTorque(new Vector3(turnTorque.x * pitch,
                                                    turnTorque.y * yaw,
                                                    -turnTorque.z * roll) * forceMult,
                                        ForceMode.Force);
            }
        }

        private void OnOutOfFuel()
        {
            isOutOfFuel = true;
            throttle = 0f;
            Debug.Log("Out of fuel! The plane has stopped thrusting.");
        }

        public float GetFuel()
        {
            return fuel;
        }

        public float GetVerticalSpeed()
        {
            return (GetAltitude() - lastAltitude) / Time.deltaTime;
        }

        public float GetAltitude()
        {
            return transform.position.y;
        }

        public float GetSpeed()
        {
            return rigid.velocity.magnitude;
        }

        public float GetHeading()
        {
            return transform.eulerAngles.y;
        }
    }
}
