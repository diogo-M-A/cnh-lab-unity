using UnityEngine;

namespace CnhTreino
{
    // Independent state machine: clutch=1 means pedal fully pressed.
    [System.Serializable]
    public class ManualPowertrain
    {
        public bool running;
        public int gear;
        public float rpm = 0;
        public float clutch = 1;
        public int stalls;
        public static readonly float[] Ratios = { 0, 3.73f, 2.05f, 1.39f, 1.03f, .82f };
        public bool Start(float speed)
        {
            if (gear != 0 && clutch < .85f) return false;
            running = true; rpm = 900; return true;
        }
        public bool Shift(int target, float speed)
        {
            if (target < -1 || target > 5 || clutch < .85f) return false;
            if ((target == -1 && speed > .5f) || (target > 0 && speed < -.5f)) return false;
            gear = target; return true;
        }
        public float Step(float dt, float throttle, float speed, float wheelRpm)
        {
            if (!running) { rpm = Mathf.MoveTowards(rpm, 0, dt * 4000); return 0; }
            float ratio = gear == -1 ? -3.31f : Ratios[gear];
            float coupled = Mathf.Abs(wheelRpm * ratio * 4.18f);
            float engagement = 1 - Mathf.SmoothStep(.12f, .85f, clutch);
            float free = 900 + throttle * 5100;
            float target = gear == 0 ? free : Mathf.Lerp(free, coupled, engagement);
            rpm = Mathf.MoveTowards(rpm, target, dt * 5500);
            if (gear != 0 && engagement > .8f && rpm < 500) { running = false; stalls++; return 0; }
            if (gear == 0) return 0;
            float torque = ((25 + throttle * 88) - Mathf.Max(0,rpm-1100)*.015f*(1-throttle)) * Mathf.Clamp01((6500 - rpm) / 1000);
            return torque * ratio * 4.18f * engagement;
        }
    }
}
