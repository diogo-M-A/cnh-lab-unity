using UnityEngine;

namespace CnhTreino
{
    public class TrainingWorld : MonoBehaviour
    {
        public CarController car;
        public Transform trafficLamp;
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
            if (!collision) Destroy(o.GetComponent<Collider>());
            return o;
        }
        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
            asphalt = Mat("Asfalto", new Color(.16f,.19f,.22f)); paint = Mat("Branco", new Color(.92f,.94f,.87f));
            grass = Mat("Jardim", new Color(.25f,.4f,.28f)); red = Mat("Carro • azul petróleo", new Color(.06f,.37f,.45f));
            glass = Mat("Vidro", new Color(.12f,.22f,.27f)); rubber = Mat("Pneu", new Color(.055f,.065f,.075f)); metal = Mat("Aço", new Color(.5f,.58f,.61f));
            RenderSettings.ambientLight = new Color(.65f,.72f,.8f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(.64f,.78f,.85f); RenderSettings.fogDensity = .0025f;
            var sun = new GameObject("Sol").AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.3f; sun.shadows = LightShadows.Soft; sun.transform.rotation = Quaternion.Euler(48,-35,0);
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
            trafficLamp = Box("Luz semáforo", new Vector3(6.7f,4,50.71f), new Vector3(.42f,.95f,.08f), paint, false).transform;
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
            var building = Mat("Fachadas", new Color(.63f,.66f,.61f));
            for (int i = 0; i < 8; i++)
            {
                float h = 4 + i%3 * 2;
                Box("Bairro", new Vector3(10+i*8,h/2,96), new Vector3(6,h,9), building);
                Box("Janela", new Vector3(10+i*8,h*.65f,91.45f), new Vector3(3,1.1f,.08f), glass, false);
            }
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
                Box("Linha central",p+d,vertical?new Vector3(.13f,.02f,2.5f):new Vector3(2.5f,.02f,.13f),paint,false);
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
            Box("Poste",p+Vector3.up*1.4f,new Vector3(.08f,2.8f,.08f),metal);
            Box("Placa "+title,p+Vector3.up*2.7f,new Vector3(1.8f,1.35f,.13f),Mat(title,color));
            int longest=1;foreach(var line in title.Split('\n'))longest=Mathf.Max(longest,line.Length);
            Label(title,p+new Vector3(0,2.7f,-.09f),Mathf.Min(.18f,.5f/longest),Quaternion.identity);
        }
        void Label(string text,Vector3 p,float scale,Quaternion rotation)
        {
            var o=new GameObject(text);o.transform.position=p;o.transform.rotation=rotation;
            var tm=o.AddComponent<TextMesh>();tm.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");tm.GetComponent<Renderer>().sharedMaterial=tm.font.material;
            tm.text=text;tm.fontSize=64;tm.characterSize=scale;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.color=Color.white;
        }
        void Tree(Vector3 p)
        {
            Box("Tronco",p+Vector3.up*1.4f,new Vector3(.5f,2.8f,.5f),metal);
            var crown=GameObject.CreatePrimitive(PrimitiveType.Sphere);crown.name="Árvore";crown.transform.position=p+Vector3.up*3.7f;crown.transform.localScale=new Vector3(3.7f,4,3.7f);crown.GetComponent<Renderer>().sharedMaterial=grass;
        }
        void ParkedCar(Vector3 p)
        {
            var o=Box("Veículo estacionado",p+Vector3.up*.65f,new Vector3(1.65f,1.1f,3.7f),metal);o.tag="Obstacle";
            Box("Cabine estacionado",p+Vector3.up*1.4f,new Vector3(1.5f,.5f,1.9f),glass);
        }
        void BuildCar()
        {
            var root=new GameObject("Hatch 1.0 • manual 5 marchas");root.transform.position=new Vector3(2.8f,.5f,0);
            root.AddComponent<Rigidbody>();car=root.AddComponent<CarController>();
            Box("Carroceria",new Vector3(0,.68f,0),new Vector3(1.66f,.56f,3.78f),red,true,root.transform);
            Box("Cabine",new Vector3(0,1.19f,-.2f),new Vector3(1.5f,.57f,1.95f),glass,false,root.transform);
            Box("Teto",new Vector3(0,1.51f,-.25f),new Vector3(1.51f,.08f,1.66f),red,false,root.transform);
            Box("Capô",new Vector3(0,.99f,1.2f),new Vector3(1.63f,.12f,1.25f),red,false,root.transform);
            Box("Coluna esquerda",new Vector3(-.73f,1.22f,.74f),new Vector3(.08f,.62f,.1f),red,false,root.transform);
            Box("Coluna direita",new Vector3(.73f,1.22f,.74f),new Vector3(.08f,.62f,.1f),red,false,root.transform);
            for(int side=-1;side<=1;side+=2)
            {
                Box("Retrovisor",new Vector3(side*.94f,1.12f,.65f),new Vector3(.22f,.18f,.31f),red,false,root.transform);
                Box("Maçaneta",new Vector3(side*.84f,.9f,-.3f),new Vector3(.035f,.04f,.2f),rubber,false,root.transform);
                Box("Farol",new Vector3(side*.55f,.85f,1.9f),new Vector3(.45f,.22f,.05f),paint,false,root.transform);
            }
            car.beams=new Light[2];
            for(int i=0;i<2;i++)
            {
                var lamp=new GameObject("Feixe do farol");lamp.transform.SetParent(root.transform,false);lamp.transform.localPosition=new Vector3(i==0?-.55f:.55f,.85f,1.96f);lamp.transform.localRotation=Quaternion.Euler(8,0,0);
                var l=lamp.AddComponent<Light>();l.type=LightType.Spot;l.range=35;l.spotAngle=55;l.intensity=2;l.enabled=false;car.beams[i]=l;
            }
            Box("Grade",new Vector3(0,.64f,1.9f),new Vector3(.65f,.2f,.06f),rubber,false,root.transform);
            car.leftLights=new[]{Box("Seta esquerda",new Vector3(-.75f,.86f,1.92f),new Vector3(.12f,.14f,.05f),Mat("Laranja E",Color.yellow),false,root.transform).GetComponent<Renderer>(),Box("Seta traseira E",new Vector3(-.73f,.9f,-1.91f),new Vector3(.2f,.14f,.05f),Mat("Laranja TE",Color.yellow),false,root.transform).GetComponent<Renderer>()};
            car.rightLights=new[]{Box("Seta direita",new Vector3(.75f,.86f,1.92f),new Vector3(.12f,.14f,.05f),Mat("Laranja D",Color.yellow),false,root.transform).GetComponent<Renderer>(),Box("Seta traseira D",new Vector3(.73f,.9f,-1.91f),new Vector3(.2f,.14f,.05f),Mat("Laranja TD",Color.yellow),false,root.transform).GetComponent<Renderer>()};
            car.brakeLights=new[]{Box("Lanterna E",new Vector3(-.62f,.73f,-1.91f),new Vector3(.3f,.17f,.05f),Mat("Vermelho E",Color.red),false,root.transform).GetComponent<Renderer>(),Box("Lanterna D",new Vector3(.62f,.73f,-1.91f),new Vector3(.3f,.17f,.05f),Mat("Vermelho D",Color.red),false,root.transform).GetComponent<Renderer>()};
            car.wheels=new WheelCollider[4];car.visuals=new Transform[4];
            for(int i=0;i<4;i++)
            {
                Vector3 pos=new Vector3(i%2==0?-.76f:.76f,.4f,i<2?1.2f:-1.14f);
                var w=new GameObject("Suspensão "+i);w.transform.SetParent(root.transform,false);w.transform.localPosition=pos;
                var wc=w.AddComponent<WheelCollider>();wc.radius=.29f;wc.mass=18;wc.suspensionDistance=.18f;
                var spring=wc.suspensionSpring;spring.spring=26000;spring.damper=3500;spring.targetPosition=.5f;wc.suspensionSpring=spring;
                var f=wc.forwardFriction;f.stiffness=1.3f;wc.forwardFriction=f;var s=wc.sidewaysFriction;s.stiffness=1.8f;wc.sidewaysFriction=s;
                car.wheels[i]=wc;
                var v=GameObject.CreatePrimitive(PrimitiveType.Cylinder);v.name="Pneu "+i;v.transform.SetParent(root.transform,false);v.transform.localScale=new Vector3(.58f,.095f,.58f);v.GetComponent<Renderer>().sharedMaterial=rubber;Destroy(v.GetComponent<Collider>());car.visuals[i]=v.transform;
            }
            car.Initialize();
        }
        void Update()
        {
            if(trafficLamp) trafficLamp.GetComponent<Renderer>().material.color=RedLight?new Color(1,.12f,.08f):new Color(.1f,1,.35f);
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
