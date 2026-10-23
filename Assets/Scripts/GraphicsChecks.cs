using UnityEngine;

namespace CnhTreino
{
    public static class GraphicsChecks
    {
        public static bool Run(CarController car)
        {
            bool all=true;
            System.Action<bool,string> check=(ok,name)=>{all&=ok;Debug.Log((ok?"GRAPHICS PASS: ":"GRAPHICS FAIL: ")+name);};
            check(car.GetComponentsInChildren<Collider>().Length==5,"malha visual mantém apenas carroceria e quatro colliders de roda");
            bool wheels=car.visuals.Length==4;
            foreach(var wheel in car.visuals)wheels&=wheel&&wheel.GetComponentsInChildren<MeshRenderer>().Length>=2&&wheel.GetComponentsInChildren<MeshRenderer>().Length<=4;
            check(wheels,"quatro rodas detalhadas com geometria combinada");
            int vertices=0;bool finite=true;
            foreach(var filter in car.GetComponentsInChildren<MeshFilter>())
            {
                vertices+=filter.sharedMesh.vertexCount;
                foreach(var p in filter.sharedMesh.vertices)if(float.IsNaN(p.sqrMagnitude)||float.IsInfinity(p.sqrMagnitude))finite=false;
            }
            check(finite&&vertices>8000,"geometria detalhada sem vértices inválidos");
            check(RenderSettings.skybox&&RenderSettings.customReflectionTexture is Cubemap&&Shader.Find("CNHLab/DepthText"),"céu, reflexos e shader de texto presentes no executável");
            check(QualitySettings.antiAliasing>=2&&QualitySettings.shadowDistance>=60,"antialiasing e sombras configurados");
            check(car.reverseLights!=null&&car.reverseLights.Length==2&&car.leftLights.Length==2&&car.rightLights.Length==2&&car.brakeLights.Length==3,"luzes e setas vinculadas ao novo modelo");
            return all;
        }
    }
}
