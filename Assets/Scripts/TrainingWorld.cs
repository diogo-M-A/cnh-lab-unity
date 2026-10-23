using UnityEngine;

namespace CnhTreino
{
    public class TrainingWorld : MonoBehaviour
    {
        public CarController car;
        public Transform trafficLamp;
        Transform trafficGreen;
        Transform pedestrian;
        public bool PedestrianInCrossing => Time.time % 30 > 2 && Time.time % 30 < 12;
        public Material asphalt, paint, grass, red, glass, rubber, metal;
        public bool RedLight => Time.time % 30 < 14;
        public static TrainingWorld Instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { if (!FindAnyObjectByType<TrainingWorld>()) new GameObject("CNH • Treino").AddComponent<TrainingWorld>(); }
        Material Mat(string name, Color color)
        { var m = new Material(Shader.Find("Standard")); m.name = name; m.color = color; return m; }
        public GameObject Box(string name, Vector3 p, Vector3 size, Material mat, bool collision = true, Transform parent = null)
        {
            var o = GameObject.CreatePrimitive(PrimitiveType.Cube); o.name = name;
            if (parent) o.transform.SetParent(parent, false);
            o.transform.localPosition = p; o.transform.localScale = size; o.GetComponent<Renderer>().sharedMaterial = mat;
            if(mat==asphalt||mat==grass)
            {var block=new MaterialPropertyBlock();block.SetVector("_MainTex_ST",new Vector4(size.x/3, size.z/3,0,0));o.GetComponent<Renderer>().SetPropertyBlock(block);}
            if (!collision) Destroy(o.GetComponent<Collider>());
            return o;
        }
        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
            asphalt = Mat("Asfalto", new Color(.29f,.3f,.31f)); paint = Mat("Branco", new Color(.92f,.94f,.87f));
            grass = Mat("Jardim", new Color(.37f,.43f,.24f)); red = Mat("Carro • azul petróleo", new Color(.06f,.37f,.45f));
            asphalt.mainTexture=StreetEnvironment.Grain("Asfalto granulado",256,32);asphalt.SetFloat("_Glossiness",.16f);
            grass.mainTexture=StreetEnvironment.Grain("Gramado",128,76);
            glass = Mat("Vidro", new Color(.12f,.22f,.27f)); rubber = Mat("Pneu", new Color(.055f,.065f,.075f)); metal = Mat("Aço", new Color(.5f,.58f,.61f));
            RenderSettings.ambientLight = new Color(.65f,.72f,.8f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(.64f,.78f,.85f); RenderSettings.fogDensity = .0025f;
            var sun = new GameObject("Sol").AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.3f; sun.shadows = LightShadows.Soft; sun.transform.rotation = Quaternion.Euler(48,-35,0);
            StreetEnvironment.Lighting(sun);
            Camera.main?.gameObject.SetActive(false);
            BuildRoads(); BuildCar();
            gameObject.AddComponent<DrivingCamera>().car = car;
            gameObject.AddComponent<TrainingSession>().world = this;
            gameObject.AddComponent<DrivingHUD>();
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-smokeTest") >= 0) gameObject.AddComponent<RuntimeSmoke>();
        }
        void BuildRoads()
        {
            Box("Terreno", new Vector3(15,-.3f,35), new Vector3(260,.5f,240), grass);
            Road(new Vector3(0,0,35), new Vector3(12,.12f,94), true);
            Road(new Vector3(60,0,35), new Vector3(12,.12f,94), true);
            Road(new Vector3(30,0,70), new Vector3(72,.12f,12), false);
            Road(new Vector3(30,0,0), new Vector3(72,.12f,12), false);
            // Parallel-parking exercise bay on the right side of the northbound lane.
            Box("Recuo estacionamento", new Vector3(8,0,12), new Vector3(5,.13f,16), asphalt);
            for (int i = 0; i < 2; i++)
            {
                Box("Limite da vaga", new Vector3(8,.09f,8.4f + i*7.2f), new Vector3(2.8f,.025f,.14f), paint, false);
                ParkedCar(new Vector3(8,.1f,4.7f + i*14.6f));
            }
            Box("Lateral da vaga", new Vector3(9.4f,.09f,12), new Vector3(.13f,.025f,7.2f), paint, false);
            StopLine(2.8f,24); Sign("PARE", new Vector3(6.8f,0,24), new Color(.7f,.12f,.13f));
            Sign("30", new Vector3(6.8f,0,9), new Color(.7f,.12f,.13f));
            StopLine(2.8f,49);
            var pole = Box("Semáforo", new Vector3(6.7f,2,51), new Vector3(.16f,4,.16f), metal);
            Box("Caixa semáforo", new Vector3(6.7f,4,51), new Vector3(.7f,1.4f,.5f), rubber);
            trafficLamp = VisualGeometry.Disc("Luz vermelha",transform,new Vector3(6.7f,4.43f,50.71f),.2f,Mat("Lente vermelha semáforo",Color.red)).transform;
            VisualGeometry.Disc("Luz amarela",transform,new Vector3(6.7f,4,50.71f),.2f,Mat("Lente amarela semáforo",new Color(.18f,.13f,.025f)));
            trafficGreen=VisualGeometry.Disc("Luz verde",transform,new Vector3(6.7f,3.57f,50.71f),.2f,Mat("Lente verde semáforo",Color.green)).transform;
            for (int i = 0; i < 7; i++) Box("Faixa de pedestres", new Vector3(-4.8f+i*1.6f,.09f,53), new Vector3(.8f,.02f,3), paint, false);
            Sign("PREFERÊNCIA\nAO PEDESTRE", new Vector3(7,0,56), new Color(.18f,.3f,.4f));
            var person=GameObject.CreatePrimitive(PrimitiveType.Capsule);person.name="Pedestre na travessia";person.tag="Obstacle";
            person.transform.localScale=new Vector3(.5f,.85f,.5f);person.GetComponent<Renderer>().sharedMaterial=Mat("Roupa pedestre",new Color(.98f,.62f,.2f));
            var prb=person.AddComponent<Rigidbody>();prb.isKinematic=true;pedestrian=person.transform;
            // Training hill, 8-degree slope, a continuous approach and a flat summit.
            Road(new Vector3(-38,0,-7), new Vector3(10,.12f,20), true);
            var ramp = Box("Rampa de treino • 8 graus", new Vector3(-38,1.8f,15), new Vector3(10,.3f,26), asphalt);
            ramp.transform.rotation = Quaternion.Euler(-8,0,0);
            Box("Topo da rampa", new Vector3(-38,3.61f,34), new Vector3(10,.3f,12), asphalt);
            Sign("RAMPA\nTREINO", new Vector3(-31,0,-5), new Color(.14f,.3f,.4f));
            for (int i = 0; i < 30; i++)
            {
                float x = i < 15 ? -14 : 76; float z = (i%15)*7-12;
                Tree(new Vector3(x,0,z));
            }
            StreetEnvironment.Build(this);
            Label("CIRCUITO DE TREINO", new Vector3(28,.16f,34), .2f, Quaternion.Euler(90,0,0));
            Label("BALIZA", new Vector3(8,.16f,12), .15f, Quaternion.Euler(90,0,0));
        }
        void Road(Vector3 p, Vector3 size, bool vertical)
        {
            Box("Via", p,size,asphalt);
            float length = vertical ? size.z : size.x;
            for (int i=0;i<length/5;i++)
            {
                Vector3 d = vertical ? new Vector3(0,.08f,-length/2+i*5+2) : new Vector3(-length/2+i*5+2,.08f,0);
                Box("Linha central",p+d,vertical?new Vector3(.13f,.02f,2.5f):new Vector3(2.5f,.02f,.13f),Mat("Amarelo viário",new Color(.95f,.7f,.1f)),false);
            }
            // Keep intersections and parking access open.
            if (vertical)
            {
                var curb = Mat("Meio-fio",new Color(.67f,.7f,.69f));
                foreach (float side in new[]{-1f,1f})
                {
                    if (p.x == 0 && side > 0) continue;
                    var c=Box("Meio-fio", new Vector3(p.x+side*6.25f,.18f,35),new Vector3(.35f,.35f,54),curb); c.tag="Obstacle";
                }
            }
        }
        void StopLine(float x,float z) { Box("Linha de retenção",new Vector3(x,.09f,z),new Vector3(5.1f,.02f,.3f),paint,false); }
        void Sign(string title, Vector3 p, Color color)
        {
            Box("Poste",p+Vector3.up*1.4f+Vector3.forward*.15f,new Vector3(.08f,2.8f,.08f),metal);
            if(title=="PARE"||title=="30")
            {
                int sides=title=="PARE"?8:48;Vector3 center=p+Vector3.up*2.7f;
                VisualGeometry.Disc("Borda da placa "+title,transform,center,.76f,paint,sides);
                VisualGeometry.Disc("Face da placa "+title,transform,center+Vector3.back*.014f,.705f,Mat(title,color),sides);
                if(title=="30")VisualGeometry.Disc("Centro branco",transform,center+Vector3.back*.027f,.575f,paint,sides);
                VisualGeometry.Text(title,transform,center+Vector3.back*.065f,title=="30"?.15f:.078f,title=="30"?Color.black:Color.white);return;
            }
            Box("Placa "+title,p+Vector3.up*2.7f,new Vector3(1.8f,1.35f,.13f),Mat(title,color));
            int longest=1;foreach(var line in title.Split('\n'))longest=Mathf.Max(longest,line.Length);
            Label(title,p+new Vector3(0,2.7f,-.09f),Mathf.Min(.18f,.5f/longest),Quaternion.identity);
        }
        void Label(string text,Vector3 p,float scale,Quaternion rotation)
        {
            var o=new GameObject(text);o.transform.position=p;o.transform.rotation=rotation;
            var tm=o.AddComponent<TextMesh>();tm.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");tm.GetComponent<Renderer>().sharedMaterial=VisualGeometry.FontMaterial(tm.font);
            tm.text=text;tm.fontSize=64;tm.characterSize=scale;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.color=Color.white;
        }
        void Tree(Vector3 p)
        {
            StreetEnvironment.Tree(p,transform);
        }
        void ParkedCar(Vector3 p)
        {
            var o=Box("Veículo estacionado",p+Vector3.up*.65f,new Vector3(1.65f,1.1f,3.7f),metal);o.tag="Obstacle";o.GetComponent<Renderer>().enabled=false;
            var parked=new GameObject("Hatch estacionado");parked.transform.position=p-Vector3.up*.06f;
            var controller=parked.AddComponent<CarController>();controller.enabled=false;CeltaVisuals.Build(controller);
            for(int i=0;i<4;i++){var wheel=CeltaVisuals.Wheel(parked.transform,i);wheel.localPosition=new Vector3(i%2==0?-.76f:.76f,.32f,i<2?1.2f:-1.14f);wheel.localRotation=Quaternion.Euler(0,0,90);}
        }
        void BuildCar()
        {
            var root=new GameObject(CeltaVisuals.ModelName);root.transform.position=new Vector3(2.8f,.5f,0);
            root.AddComponent<Rigidbody>();car=root.AddComponent<CarController>();
            var collision=root.AddComponent<BoxCollider>();collision.center=new Vector3(0,.68f,0);collision.size=new Vector3(1.66f,.56f,3.78f);
            CeltaVisuals.Build(car);
            car.wheels=new WheelCollider[4];car.visuals=new Transform[4];
            for(int i=0;i<4;i++)
            {
                var wheel=new GameObject("Suspensão "+i);wheel.transform.SetParent(root.transform,false);wheel.transform.localPosition=new Vector3(i%2==0?-.76f:.76f,.4f,i<2?1.2f:-1.14f);
                var wc=wheel.AddComponent<WheelCollider>();wc.radius=.29f;wc.mass=18;wc.suspensionDistance=.18f;
                var spring=wc.suspensionSpring;spring.spring=26000;spring.damper=3500;spring.targetPosition=.5f;wc.suspensionSpring=spring;
                var forward=wc.forwardFriction;forward.stiffness=1.3f;wc.forwardFriction=forward;
                var sideways=wc.sidewaysFriction;sideways.stiffness=1.8f;wc.sidewaysFriction=sideways;
                car.wheels[i]=wc;car.visuals[i]=CeltaVisuals.Wheel(root.transform,i);
            }
            car.Initialize();
        }
        void Update()
        {
            if(trafficLamp) trafficLamp.GetComponent<Renderer>().material.color=RedLight?new Color(1,.08f,.025f):new Color(.13f,.015f,.01f);
            if(trafficGreen)trafficGreen.GetComponent<Renderer>().material.color=RedLight?new Color(.015f,.1f,.025f):new Color(.1f,1,.26f);
        }
        void FixedUpdate()
        {
            if(pedestrian)
            {
                float phase=Time.time%30;
                float x=phase<2?-7:phase>12?7:Mathf.Lerp(-7,7,(phase-2)/10);
                pedestrian.GetComponent<Rigidbody>().MovePosition(new Vector3(x,.94f,53));
            }
        }
    }
}
