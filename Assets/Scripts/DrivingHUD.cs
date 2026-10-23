using UnityEngine;

namespace CnhTreino
{
    public class DrivingHUD:MonoBehaviour
    {
        TrainingSession session;
        DrivingCamera rig;
        CarController car;
        bool help=true;
        GUIStyle title,label,small,big,button,heading;
        Texture2D pixel;
        readonly Color ink=new Color(.93f,.97f,.98f),muted=new Color(.8f,.87f,.9f),accent=new Color(.3f,.96f,.79f),panel=new Color(.035f,.07f,.095f,.97f);
        void Start(){session=GetComponent<TrainingSession>();rig=GetComponent<DrivingCamera>();car=session.Car;}
        void Update(){if(Input.GetKeyDown(KeyCode.F1))help=!help;}
        void Styles()
        {
            if(pixel)return;pixel=Texture2D.whiteTexture;
            label=new GUIStyle(GUI.skin.label){fontSize=15,wordWrap=true};label.normal.textColor=ink;
            small=new GUIStyle(label){fontSize=15};small.normal.textColor=muted;
            heading=new GUIStyle(label){fontSize=14,fontStyle=FontStyle.Bold};heading.normal.textColor=accent;
            title=new GUIStyle(label){fontSize=26,fontStyle=FontStyle.Bold};big=new GUIStyle(title){fontSize=48};
            button=new GUIStyle(GUI.skin.button){fontSize=13,padding=new RectOffset(8,8,8,8)};button.normal.textColor=ink;
        }
        void Panel(Rect r){GUI.color=panel;GUI.DrawTexture(r,pixel);GUI.color=Color.white;}
        void Text(float x,float y,float w,float h,string t,GUIStyle s=null){GUI.Label(new Rect(x,y,w,h),t,s??label);}
        bool Btn(float x,float y,float w,string t){return GUI.Button(new Rect(x,y,w,32),t,button);}
        void Meter(float x,float y,string name,float v,Color color)
        {
            Text(x,y,85,20,name,small);GUI.color=new Color(.13f,.21f,.25f);GUI.DrawTexture(new Rect(x,y+24,82,5),pixel);GUI.color=color;GUI.DrawTexture(new Rect(x,y+24,82*Mathf.Clamp01(v),5),pixel);GUI.color=Color.white;
        }
        void Status(float x,float y,string t,bool on){var before=small.normal.textColor;small.normal.textColor=on?accent:new Color(1,.77f,.5f);Text(x,y,175,26,(on?"✓ ":"○ ")+t,small);small.normal.textColor=before;}
        void OnGUI()
        {
            if(!session||!car)return;Styles();
            float scale=Mathf.Min(Screen.width/1440f,Screen.height/900f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1440*scale)/2,(Screen.height-900*scale)/2,0),Quaternion.identity,Vector3.one*scale);
            Panel(new Rect(20,20,1400,84));Text(42,32,350,36,"CNH LAB",title);Text(43,70,380,22,"DIREÇÃO MANUAL  /  CATEGORIA B",small);
            Text(510,38,480,30,session.ExerciseNames[session.exercise],title);Text(512,72,600,20,"HATCH INSPIRADO NO CELTA   •   MANUAL 1.0   •   30 KM/H",small);
            if(Btn(1120,44,140,help?"Ocultar ajuda [F1]":"Ajuda [F1]"))help=!help;
            if(Btn(1270,44,125,"Reiniciar [F2]"))session.Reset(session.exercise);
            Panel(new Rect(20,120,315,help?610:290));Text(40,140,270,24,"SEU TREINO",heading);
            for(int i=0;i<4;i++)if(Btn(40,174+i*40,275,(session.exercise==i?"›  ":"")+session.ExerciseNames[i]))session.Reset(i);
            if(Btn(40,340,275,session.exam?"Modo: avaliação":"Modo: guiado")){session.exam=!session.exam;session.Reset(session.exercise);}
            if(help)
            {
                Text(40,394,270,22,"COMANDOS",heading);
                Text(40,426,274,265,"W / ↑   acelerar\nS / ↓   freio de serviço\nA D / ← →   direção\nShift (segurar)   embreagem\n1–5 / N ou 0 / R   marchas\nI   ligar / desligar motor\nEspaço   freio de mão\nQ / E   seta esquerda / direita\nB   cinto     J   ajustar banco\nM   ajustar espelhos     O   porta\nZ / X   observar esq. / dir.\nC   câmera     V   olhar para trás\nL   luzes     H   pisca-alerta\nEsc   pausa",small);
            }
            // Live side mirrors, horizontally reflected like real mirrors.
            Panel(new Rect(365,120,230,132));Panel(new Rect(1160,120,260,132));
            if(rig.leftMirror){GUI.DrawTextureWithTexCoords(new Rect(375,130,210,98),rig.leftMirror,new Rect(1,0,-1,1));GUI.DrawTextureWithTexCoords(new Rect(1170,130,240,98),rig.rightMirror,new Rect(1,0,-1,1));}
            Text(375,230,210,20,"RETROVISOR ESQUERDO",small);Text(1170,230,235,20,"RETROVISOR DIREITO",small);
            Panel(new Rect(1010,274,410,236));Text(1030,290,370,22,"PREPARAÇÃO DO VEÍCULO",heading);
            Status(1030,326,"Cinto",car.belt);Status(1220,326,"Porta fechada",car.doorClosed);
            Status(1030,354,"Banco",car.seat);Status(1220,354,"Retrovisores",car.mirrors);
            Status(1030,382,"Motor ligado",car.motor.running);Status(1220,382,"Freio mão solto",!car.handbrake);
            Text(1030,422,368,66,session.exam?"Avaliação: execute o exercício sem dicas. As ocorrências aparecem no relatório.":session.notice,small);
            if(!session.exam)
            {Panel(new Rect(615,120,525,132));Text(635,134,480,24,"PRÓXIMA AÇÃO",heading);Text(635,166,480,76,session.Instruction);}
            Panel(new Rect(20,750,1400,130));
            Text(44,768,135,62,car.Kph.ToString("00"),big);Text(45,833,120,24,"KM/H  •  LIMITE 30",small);
            Text(218,766,100,62,car.motor.gear==-1?"R":car.motor.gear==0?"N":car.motor.gear.ToString(),big);Text(218,833,120,24,"MARCHA",small);
            Text(341,778,190,40,$"{car.motor.rpm:0} rpm",title);Text(341,831,210,25,car.motor.running?"MOTOR LIGADO":"MOTOR DESLIGADO",small);
            Meter(570,790,"EMBREAGEM",car.motor.clutch,accent);Meter(680,790,"FREIO",car.brake,new Color(1,.47f,.38f));Meter(790,790,"ACELERADOR",car.throttle,accent);
            GUI.color=car.signal==-1||car.hazards?accent:muted;Text(900,790,50,36,"◀",title);GUI.color=car.signal==1||car.hazards?accent:muted;Text(960,790,50,36,"▶",title);GUI.color=Color.white;
            Text(1040,777,355,40,$"{session.elapsed/60:00}:{session.elapsed%60:00}   /   {session.points} pts",title);
            Text(1040,818,340,40,"Shift: pressione para desacoplar.\nSolte aos poucos para encontrar o ponto.",small);
            if(session.exercise==0)Map();
            if(car.paused||session.complete)Summary();
        }
        void Map()
        {
            Panel(new Rect(1210,530,210,180));Text(1228,540,180,20,"PERCURSO",heading);
            GUI.color=new Color(.21f,.3f,.34f);GUI.DrawTexture(new Rect(1240,580,135,6),pixel);GUI.DrawTexture(new Rect(1240,680,135,6),pixel);GUI.DrawTexture(new Rect(1240,580,6,106),pixel);GUI.DrawTexture(new Rect(1370,580,6,106),pixel);
            Vector3 p=car.transform.position;GUI.color=accent;GUI.DrawTexture(new Rect(1240+p.x/60*130-4,680-p.z/70*100-4,9,9),pixel);
            Vector3 t=session.Target;GUI.color=Color.yellow;GUI.DrawTexture(new Rect(1240+t.x/60*130-3,680-t.z/70*100-3,6,6),pixel);GUI.color=Color.white;
        }
        void Summary()
        {
            Panel(new Rect(350,270,650,300));Text(377,290,600,42,session.complete?"Exercício concluído":"Treino pausado",title);
            Text(377,338,600,24,$"{session.points} pontos didáticos  •  {session.elapsed:0} segundos",label);
            Text(377,374,595,95,session.events.Count==0?"Nenhuma ocorrência registrada.":string.Join("\n",session.events.GetRange(0,Mathf.Min(4,session.events.Count)).ToArray()),small);
            if(Btn(377,478,180,"Salvar relatório"))session.SaveReport();
            if(Btn(568,478,180,"Recomeçar")){car.paused=false;Time.timeScale=1;session.Reset(session.exercise);}
            if(!session.complete&&Btn(759,478,200,"Continuar [Esc]")){car.paused=false;Time.timeScale=1;}
            Text(377,522,595,40,session.reportPath.Length>0?"Relatório salvo em: "+session.reportPath:"Ferramenta de estudo. A avaliação usa critérios próprios do jogo.",small);
        }
    }
}
