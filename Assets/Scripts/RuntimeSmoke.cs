using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CnhTreino
{
    // All driving phases use the production CarController.FixedUpdate path.
    public class RuntimeSmoke:MonoBehaviour
    {
        readonly List<string> results=new List<string>();
        CarController car;
        TrainingSession session;
        bool passed=true;
        void Check(bool condition,string name)
        { passed&=condition;results.Add((condition?"PASS: ":"FAIL: ")+name);Debug.Log(results[results.Count-1]); }
        void Prepare(Vector3 position,int gear)
        {
            car.ResetAt(position,Quaternion.identity);car.externalInput=true;
            car.belt=car.seat=car.mirrors=true;car.Observe();car.handbrake=false;
            car.motor.clutch=1;car.motor.Start(0);car.motor.Shift(gear,0);
        }
        IEnumerator Start()
        {
            car=TrainingWorld.Instance.car;session=GetComponent<TrainingSession>();
            yield return new WaitForSeconds(1.5f);
            Check(session.points==0,"veículo parado não recebe faltas ao nascer");
            Check(GraphicsChecks.Run(car),"seis verificações estruturais dos gráficos");
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-nographics")<0)
            { CaptureScene(); }
            session.enabled=false;
            Prepare(new Vector3(2.8f,.5f,0),1);
            float maxSpeed=0;Vector3 origin=car.transform.position;
            for(int i=0;i<350;i++)
            {
                car.throttle=.55f;car.motor.clutch=Mathf.MoveTowards(car.motor.clutch,0,.005f);
                maxSpeed=Mathf.Max(maxSpeed,car.Kph);yield return new WaitForFixedUpdate();
            }
            Check(maxSpeed>5&&car.transform.position.z>origin.z+3&&car.motor.running,"arrancada pelo controlador de produção");
            car.throttle=0;car.brake=1;car.motor.clutch=1;
            for(int i=0;i<150;i++)yield return new WaitForFixedUpdate();
            Check(car.Kph<.5f,"freio de serviço imobiliza o carro");
            Prepare(new Vector3(2.8f,.5f,0),-1);origin=car.transform.position;
            for(int i=0;i<200;i++)
            { car.throttle=.4f;car.motor.clutch=Mathf.MoveTowards(car.motor.clutch,0,.008f);yield return new WaitForFixedUpdate(); }
            Check(car.transform.position.z<origin.z-2&&car.Speed<-.5f,"ré movimenta o carro para trás");
            Prepare(new Vector3(2.8f,.5f,0),1);car.steering=.65f;
            for(int i=0;i<180;i++)
            { car.throttle=.35f;car.motor.clutch=Mathf.MoveTowards(car.motor.clutch,0,.006f);yield return new WaitForFixedUpdate(); }
            Check(Mathf.Abs(Mathf.DeltaAngle(car.transform.eulerAngles.y,0))>10,"direção altera a trajetória física");
            Prepare(new Vector3(-38,2.3f,14),1);car.handbrake=true;
            for(int i=0;i<75;i++)yield return new WaitForFixedUpdate();
            origin=car.transform.position;
            for(int i=0;i<240;i++)
            {
                car.throttle=.75f;car.motor.clutch=Mathf.MoveTowards(car.motor.clutch,0,.006f);
                if(car.motor.clutch<.5f)car.handbrake=false;
                yield return new WaitForFixedUpdate();
            }
            Check(car.transform.position.z>origin.z+3&&car.motor.running,"retomada real em rampa de 8 graus");
            car.brake=1;car.motor.clutch=1;car.throttle=0;
            Check(ScenarioChecks.Run(session),"regras e regressões dos exercícios");
            string report="CNH LAB • TESTE INTEGRADO\n"+string.Join("\n",results)+$"\nPico no ensaio: {maxSpeed:F2} km/h\nResultado: {(passed?"PASS":"FAIL")}\n";
            File.WriteAllText(Path.Combine(Application.dataPath,"../../runtime-smoke.txt"),report);
            Debug.Log(report);Application.Quit(passed?0:1);
        }
        void CaptureScene()
        {
            var camera=GetComponent<DrivingCamera>().view;
            var rt=new RenderTexture(1440,900,24);rt.Create();
            var old=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var texture=new Texture2D(1440,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1440,900),0,0);texture.Apply();
            File.WriteAllBytes(Path.Combine(Application.dataPath,"../../scene-preview.png"),texture.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=old;rt.Release();Destroy(rt);Destroy(texture);
            var position=camera.transform.position;var rotation=camera.transform.rotation;float fov=camera.fieldOfView;
            CaptureAngle(camera,new Vector3(-4.4f,2.25f,5.5f),"celta-front-v0.3.0.png");
            CaptureAngle(camera,new Vector3(-4.3f,2.4f,-5.5f),"celta-rear-v0.3.0.png");
            camera.transform.SetPositionAndRotation(position,rotation);camera.fieldOfView=fov;
        }
        void CaptureAngle(Camera camera,Vector3 offset,string filename)
        {
            camera.transform.position=car.transform.TransformPoint(offset);camera.transform.LookAt(car.transform.position+Vector3.up*.85f);camera.fieldOfView=43;
            var target=new RenderTexture(1440,900,24){antiAliasing=4};target.Create();var old=RenderTexture.active;
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var image=new Texture2D(1440,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1440,900),0,0);image.Apply();
            File.WriteAllBytes(Path.Combine(Application.dataPath,"../../"+filename),image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=old;target.Release();Destroy(target);Destroy(image);
        }
    }
}
