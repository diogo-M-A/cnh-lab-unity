using UnityEngine;

namespace CnhTreino
{
    public class DrivingCamera:MonoBehaviour
    {
        public CarController car;
        public Camera view;
        public RenderTexture leftMirror,rightMirror;
        public int mode;
        public void Start()
        {
            view=new GameObject("Câmera do aluno").AddComponent<Camera>();view.tag="MainCamera";view.fieldOfView=65;view.nearClipPlane=.06f;view.farClipPlane=350;
            view.backgroundColor=new Color(.64f,.78f,.85f);view.clearFlags=CameraClearFlags.SolidColor;
            view.gameObject.AddComponent<AudioListener>();
            leftMirror=Mirror("Retrovisor esquerdo",new Vector3(-.96f,1.25f,.7f),-155);
            rightMirror=Mirror("Retrovisor direito",new Vector3(.96f,1.25f,.7f),155);
        }
        RenderTexture Mirror(string name,Vector3 p,float yaw)
        {
            var o=new GameObject(name);o.transform.SetParent(car.transform,false);o.transform.localPosition=p;o.transform.localRotation=Quaternion.Euler(6,yaw,0);
            var c=o.AddComponent<Camera>();c.fieldOfView=65;c.nearClipPlane=.1f;c.farClipPlane=160;c.depth=-2;
            c.backgroundColor=new Color(.64f,.78f,.85f);c.clearFlags=CameraClearFlags.SolidColor;
            var rt=new RenderTexture(320,160,16);rt.Create();c.targetTexture=rt;return rt;
        }
        void Update() { if(Input.GetKeyDown(KeyCode.C)) mode=(mode+1)%3; }
        void LateUpdate()
        {
            if(!car || !view) return;
            if(mode==0)
            {
                Vector3 p=car.transform.TransformPoint(new Vector3(0,3.5f,-6.7f));
                view.transform.position=Vector3.Lerp(view.transform.position,p,1-Mathf.Exp(-Time.unscaledDeltaTime*7));
                view.transform.LookAt(car.transform.position+Vector3.up*1.1f+car.transform.forward*2);
            }
            else if(mode==1)
            {
                // Above the hood, outside the opaque procedural windshield.
                view.transform.position=car.transform.TransformPoint(new Vector3(-.38f,1.4f,.86f));
                float yaw=Input.GetKey(KeyCode.Z)?-80:Input.GetKey(KeyCode.X)?80:Input.GetKey(KeyCode.V)?180:0;
                if(Input.GetKey(KeyCode.V))view.transform.position=car.transform.TransformPoint(new Vector3(0,1.35f,-2.05f));
                view.transform.rotation=car.transform.rotation*Quaternion.Euler(5,yaw,0);
            }
            else { view.transform.position=car.transform.position+Vector3.up*22;view.transform.rotation=Quaternion.Euler(90,0,0); }
        }
        void OnDestroy() { if(leftMirror) leftMirror.Release();if(rightMirror) rightMirror.Release(); }
    }
}
