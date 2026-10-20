using UnityEngine;

namespace CnhTreino
{
    public class CarController : MonoBehaviour
    {
        public ManualPowertrain motor = new ManualPowertrain();
        public Rigidbody body;
        public WheelCollider[] wheels;
        public Transform[] visuals;
        public Renderer[] leftLights, rightLights, brakeLights;
        public Light[] beams;
        public bool belt, seat, mirrors, doorClosed = true, handbrake = true, headlights, hazards;
        public int signal;
        public float throttle, brake, steering;
        public bool paused;
        [System.NonSerialized] public bool externalInput;
        public System.Action<string, int> fault;
        public float Speed => Vector3.Dot(body.linearVelocity, transform.forward);
        public float Kph => Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude * 3.6f;
        public bool LookedRecently => Time.time - lookedAt < 7;
        float lookedAt = -100;
        AudioSource audioSource;
        public void Initialize()
        {
            body = GetComponent<Rigidbody>();
            body.mass = 920; body.centerOfMass = new Vector3(0, .35f, 0);
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            audioSource = gameObject.AddComponent<AudioSource>(); audioSource.loop = true;
            var clip = AudioClip.Create("Motor sintetizado", 24000, 1, 24000, false);
            var samples = new float[24000];
            for (int i = 0; i < samples.Length; i++) samples[i] = (Mathf.Sin(i * Mathf.PI * 2 * 55 / 24000) + .3f * Mathf.Sin(i * Mathf.PI * 2 * 110 / 24000)) * .18f;
            clip.SetData(samples, 0); audioSource.clip = clip; audioSource.Play();
        }
        public void Observe() { lookedAt = Time.time; }
        public void Shift(int gear) { if (!motor.Shift(gear, Speed)) fault?.Invoke("Troca recusada: pressione toda a embreagem; pare para inverter o sentido.", 0); }
        void Update()
        {
            if (paused) return;
            if (!externalInput)
            {
            float dt = Time.deltaTime;
            throttle = Mathf.MoveTowards(throttle, Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0, dt * 2);
            brake = Mathf.MoveTowards(brake, Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0, dt * 4);
            motor.clutch = Mathf.MoveTowards(motor.clutch, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 1 : 0, dt * (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 4 : 1.1f));
            float steer = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            steering = Mathf.MoveTowards(steering, steer, dt * 1.8f);
            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.N)) Shift(0);
            if (Input.GetKeyDown(KeyCode.Alpha1)) Shift(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) Shift(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) Shift(3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) Shift(4);
            if (Input.GetKeyDown(KeyCode.Alpha5)) Shift(5);
            if (Input.GetKeyDown(KeyCode.R)) Shift(-1);
            if (Input.GetKeyDown(KeyCode.I)) { if (motor.running) motor.running = false; else if (!motor.Start(Speed)) fault?.Invoke("Partida: use ponto morto ou pressione a embreagem.", 0); }
            if (Input.GetKeyDown(KeyCode.B)) belt = !belt;
            if (Input.GetKeyDown(KeyCode.J)) seat = !seat;
            if (Input.GetKeyDown(KeyCode.M)) mirrors = !mirrors;
            if (Input.GetKeyDown(KeyCode.O) && Kph < .5f) doorClosed = !doorClosed;
            if (Input.GetKeyDown(KeyCode.Space)) handbrake = !handbrake;
            if (Input.GetKeyDown(KeyCode.Q)) signal = signal == -1 ? 0 : -1;
            if (Input.GetKeyDown(KeyCode.E)) signal = signal == 1 ? 0 : 1;
            if (Input.GetKeyDown(KeyCode.H)) hazards = !hazards;
            if (Input.GetKeyDown(KeyCode.L)) headlights = !headlights;
            if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X)) Observe();
            }
            bool blink = Time.time % 1 < .5f;
            SetLights(leftLights, blink && (signal == -1 || hazards), Color.yellow);
            SetLights(rightLights, blink && (signal == 1 || hazards), Color.yellow);
            SetLights(brakeLights, brake > .05f || handbrake || headlights, Color.red);
            if(beams!=null)foreach(var beam in beams)beam.enabled=headlights;
            audioSource.pitch = Mathf.Lerp(.65f, 3, motor.rpm / 6500); audioSource.volume = motor.running ? .23f : 0;
        }
        void SetLights(Renderer[] lights, bool on, Color color)
        { foreach (var lamp in lights) lamp.material.color = on ? color * 1.6f : color * .18f; }
        void FixedUpdate()
        {
            if (paused) return;
            float torque = motor.Step(Time.fixedDeltaTime, throttle, Speed, (wheels[0].rpm + wheels[1].rpm) * .5f);
            for (int i = 0; i < 4; i++)
            {
                wheels[i].motorTorque = i < 2 ? torque * .5f : 0;
                wheels[i].brakeTorque = brake * 1700 + (handbrake && i >= 2 ? 3000 : 0);
                wheels[i].steerAngle = i < 2 ? steering * Mathf.Lerp(34, 13, Kph / 80) : 0;
                wheels[i].GetWorldPose(out Vector3 p, out Quaternion q);
                visuals[i].position = p; visuals[i].rotation = q * Quaternion.Euler(0, 0, 90);
            }
            body.AddForce(-body.linearVelocity * body.linearVelocity.magnitude * .42f);
        }
        void OnCollisionEnter(Collision collision)
        { if (collision.relativeVelocity.magnitude > .6f && collision.gameObject.CompareTag("Obstacle")) fault?.Invoke("Contato com obstáculo ou meio-fio.", 4); }
        public void ResetAt(Vector3 position, Quaternion rotation)
        {
            body.position = position; body.rotation = rotation; body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero;
            motor = new ManualPowertrain(); belt = seat = mirrors = false; doorClosed = true; handbrake = true;
            signal = 0; hazards = false; steering = throttle = brake = 0; lookedAt = -100;
            headlights = false; externalInput = false;
            foreach(var wheel in wheels){wheel.motorTorque=0;wheel.brakeTorque=3000;wheel.steerAngle=0;}
        }
    }
}
