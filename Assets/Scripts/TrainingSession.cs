using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CnhTreino
{
    public class TrainingSession:MonoBehaviour
    {
        public TrainingWorld world;
        public CarController Car=>world.car;
        public int exercise,stage,points;
        public bool exam,complete;
        public float elapsed;
        public string notice="Prepare o veículo antes de sair.",reportPath="";
        public readonly List<string> events=new List<string>();
        readonly Dictionary<string,float> cooldown=new Dictionary<string,float>();
        float stoppedAtSign,parkTime,hillStop,rollbackOrigin;
        int stalls;
        bool moving,stopCrossed,lightCrossed,hillHeld,stopSatisfied;
        Vector3 previous;
        Vector3[] route={new Vector3(2.8f,0,62),new Vector3(12,0,67.2f),new Vector3(56.9f,0,67.2f),new Vector3(57.2f,0,57),new Vector3(57.2f,0,6),new Vector3(48,0,2.8f),new Vector3(12,0,2.8f)};
        public Vector3 Target=>route[Mathf.Min(stage,route.Length-1)];
        public string[] ExerciseNames={"Percurso urbano","Baliza / estacionamento","Saída em rampa","Condução livre"};
        public string Instruction
        {
            get
            {
                if(complete)return "Treino concluído. Veja o relatório ou reinicie para praticar.";
                if(exercise==0&&stage>=6)return "Finalize no marcador: pare, ponto morto, freio de mão e desligue o motor.";
                if(!Car.belt||!Car.seat||!Car.mirrors||!Car.doorClosed)return "Preparação: banco [J], espelhos [M], cinto [B] e porta fechada [O].";
                if(!Car.motor.running)return "Pressione Shift (embreagem) e I para ligar o motor. Engate 1 para sair.";
                if(Car.handbrake)return "Com freio de serviço pressionado, solte o freio de mão [Espaço].";
                if(exercise==1)return "Sinalize à direita, observe [X] e estacione entre os carros, dentro das linhas. Finalize em N + freio de mão.";
                if(exercise==2)return hillHeld?"Saia suavemente: 1ª, acelere e solte a embreagem aos poucos. Evite recuar. Avance até o topo.":"Suba e pare no meio da rampa (entre 10 e 22 m). Segure por 3 s com o freio de mão.";
                if(exercise==3)return "Explore o circuito. Respeite 30 km/h, sinalização e pedestres. F2 reinicia.";
                if(!stopCrossed)return "Siga em frente. Pare ANTES da linha PARE por pelo menos 2 s.";
                if(!lightCrossed)return "Semáforo à frente: aguarde o verde antes da linha. Observe a travessia.";
                if(stage==0||stage==2||stage==4)return "Próxima conversão à direita: seta [E], observe [X], reduza e acompanhe o marcador.";
                if(stage>=6)return "Finalize no marcador: pare, ponto morto, freio de mão e desligue o motor.";
                return "Siga na faixa da direita até o próximo marcador. Limite didático: 30 km/h.";
            }
        }
        void Start(){Car.fault=Fault;Reset(0);}
        public void Reset(int type)
        {
            Car.paused=false;Time.timeScale=1;
            exercise=type;stage=points=stalls=0;elapsed=stoppedAtSign=parkTime=hillStop=0;complete=moving=stopCrossed=lightCrossed=hillHeld=stopSatisfied=false;
            cooldown.Clear();events.Clear();reportPath="";
            Vector3 p=type==1?new Vector3(3,.5f,23):type==2?new Vector3(-38,.5f,-9):new Vector3(2.8f,.5f,0);
            Car.ResetAt(p,Quaternion.identity);previous=p;notice="Treino iniciado. Prepare o veículo.";
            if(marker)marker.SetActive(type==0);
        }
        public void Fault(string message,int weight)
        {
            if(complete)return;
            if(cooldown.TryGetValue(message,out float last)&&Time.time-last<8)return;
            cooldown[message]=Time.time;notice=message;points+=weight;
            events.Insert(0,$"{elapsed:000}s • {message}"+(weight>0?$" (+{weight})":""));
        }
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F2))Reset(exercise);
            if(Input.GetKeyDown(KeyCode.Escape)&&!complete){Car.paused=!Car.paused;Time.timeScale=Car.paused?0:1;}
            if(Car.paused||complete)return;
            elapsed+=Time.deltaTime;Vector3 p=Car.transform.position;
            if(Car.Kph>1)
            {
                if(!Car.belt)Fault("Movimento sem cinto de segurança.",4);
                if(!Car.doorClosed)Fault("Movimento com porta aberta.",4);
                if(Car.handbrake)Fault("Freio de mão acionado durante o movimento.",2);
                if(!moving){if(!Car.seat||!Car.mirrors)Fault("Prepare banco e retrovisores antes de sair.",1);if(!Car.LookedRecently)Fault("Observe o entorno antes de sair [Z / X].",2);moving=true;}
            }
            if(Car.Kph>30.5f)Fault("Velocidade acima do limite de treino de 30 km/h.",4);
            if(Car.motor.stalls>stalls){stalls=Car.motor.stalls;Fault("Motor apagou. Freie, pressione a embreagem e dê nova partida.",0);}
            if(p.y< -3||Mathf.Abs(p.x)>120||Mathf.Abs(p.z)>130) {Fault("Fora da área de treino. Use F2 para recomeçar.",0);}
            if(exercise==0)Urban(p);
            if(exercise==1)Parking(p);
            if(exercise==2)Hill(p);
            previous=p;
        }
        void Urban(Vector3 p)
        {
            // Front bumper crosses the retention line, not the vehicle center.
            float front=p.z+1.9f,oldFront=previous.z+1.9f;
            if(!stopCrossed&&p.x>0&&p.x<6&&front>17&&front<24&&Car.Kph<.4f)
            {stoppedAtSign+=Time.deltaTime;if(stoppedAtSign>=2)stopSatisfied=true;}
            else if(!stopCrossed)stoppedAtSign=0;
            if(!stopCrossed&&oldFront<24&&front>=24&&p.x>0&&p.x<6)
            {stopCrossed=true;if(!stopSatisfied)Fault("PARE: faltou parada completa de 2 s antes da linha.",6);else notice="Parada completa registrada. Siga com atenção.";}
            if(!lightCrossed&&oldFront<49&&front>=49&&p.x>0&&p.x<6)
            {lightCrossed=true;if(world.RedLight)Fault("Cruzou a linha com semáforo vermelho.",6);else notice="Travessia no verde. Prepare a conversão à direita.";if(!Car.LookedRecently)Fault("Observe os dois lados da travessia [Z / X].",2);}
            if(front>51.5f&&front<55&&p.x>0&&p.x<6&&Car.Kph>1&&world.PedestrianInCrossing)Fault("Não deu preferência ao pedestre na faixa.",4);
            Vector3 flat=p;flat.y=0;
            if(Vector3.Distance(flat,Target)<4)
            {
                if(stage==0||stage==2||stage==4)
                {if(Car.signal!=1||Car.hazards)Fault("Conversão: use a seta direita, sem pisca-alerta.",4);if(!Car.LookedRecently)Fault("Observe o retrovisor antes da conversão.",2);}
                if(stage<6){stage++;notice="Etapa registrada. Continue até o marcador.";}
                else if(Car.Kph<.4f&&Car.handbrake&&Car.motor.gear==0&&!Car.motor.running)Finish();
            }
            // Zone checks only on straight stretches, excluding intersections.
            if(p.z>10&&p.z<58&&p.x>-5&&p.x<0)Fault("Contramão: mantenha-se à direita da linha central.",6);
            if(p.z>12&&p.z<57&&p.x>60&&p.x<65)Fault("Contramão: mantenha-se à direita da linha central.",6);
            bool onRoad=(Mathf.Abs(p.x)<6.1f||Mathf.Abs(p.x-60)<6.1f)&&(p.z>-6&&p.z<77)||(Mathf.Abs(p.z)<6.1f||Mathf.Abs(p.z-70)<6.1f)&&(p.x>-6&&p.x<66)||p.x>5&&p.x<10.5f&&p.z>4&&p.z<20;
            if(!onRoad&&Car.Kph>2)Fault("Saída da área pavimentada.",4);
            if(Car.Kph>5&&Car.motor.gear==0)Fault("Evite percorrer o circuito em ponto morto.",1);
        }
        void Parking(Vector3 p)
        {
            float angle=Mathf.Abs(Mathf.DeltaAngle(Car.transform.eulerAngles.y,0));
            bool inside=Mathf.Abs(p.x-8)<.52f&&Mathf.Abs(p.z-12)<1.55f&&angle<8;
            if(inside&&Car.Kph<.3f&&Car.handbrake&&Car.motor.gear==0)parkTime+=Time.deltaTime;else parkTime=0;
            if(parkTime>2)Finish();
        }
        void Hill(Vector3 p)
        {
            bool inLane=Mathf.Abs(p.x+38)<4;
            if(!hillHeld&&inLane&&p.z>10&&p.z<22&&Car.Kph<.4f&&Car.handbrake)hillStop+=Time.deltaTime;
            else if(!hillHeld)hillStop=0;
            if(hillStop>=3&&!hillHeld){hillHeld=true;rollbackOrigin=p.z;notice="Parada na rampa registrada. Prepare o ponto da embreagem.";}
            if(hillHeld)
            {
                rollbackOrigin=Mathf.Max(rollbackOrigin,p.z);
                if(p.z<rollbackOrigin-.5f){Fault("Recuo superior a 50 cm na rampa.",2);rollbackOrigin=p.z;}
                if(inLane&&p.z>31&&p.z<40)Finish();
            }
        }
        void Finish(){complete=true;Car.paused=true;Time.timeScale=0;notice="Exercício concluído. Relatório disponível.";SaveReport();}
        public void SaveReport()
        {
            reportPath=Path.Combine(Application.persistentDataPath,"ultimo-treino.txt");
            var text=$"CNH LAB — RELATÓRIO DIDÁTICO\n{System.DateTime.Now:yyyy-MM-dd HH:mm}\n{ExerciseNames[exercise]} | {(exam?"Avaliação":"Guiado")}\nDuração: {elapsed:0} s\nPontos didáticos: {points}\nConcluído: {complete}\n\n";
            text+=string.Join("\n",events.ToArray());
            text+="\n\nPontuação própria do jogo. Não é resultado oficial do DETRAN nem certificação.\n";
            try{File.WriteAllText(reportPath,text);}catch(System.Exception e){notice="Não foi possível salvar: "+e.Message;}
        }
        void OnDrawGizmos(){if(world&&exercise==0){Gizmos.color=Color.cyan;Gizmos.DrawWireSphere(Target,3);}}
        void LateUpdate()
        {
            if(exercise!=0)return;
            if(!marker){marker=GameObject.CreatePrimitive(PrimitiveType.Cylinder);Destroy(marker.GetComponent<Collider>());marker.name="Próximo objetivo";marker.GetComponent<Renderer>().material.color=new Color(.1f,.85f,.75f);}
            marker.transform.position=Target+Vector3.up*.15f;marker.transform.localScale=new Vector3(2.7f,.025f,2.7f);marker.SetActive(!complete);
        }
        GameObject marker;
    }
}
