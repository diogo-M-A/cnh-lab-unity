using System.Collections.Generic;
using UnityEngine;
using static CnhTreino.VisualGeometry;

namespace CnhTreino
{
    public static class CeltaVisuals
    {
        public const string ModelName="Hatch inspirado no Celta • 4 portas";
        public static int BodyVertexCount {get;private set;}
        static Material paint,glass,black,chrome,tire,lens,gold,white,red,amber;
        static Transform body;
        static float Width(float z) => Mathf.Lerp(.82f,.72f,Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.4f,1.88f,Mathf.Abs(z))));
        static float Belt(float z) => .965f-.115f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.9f,1.86f,z));
        public static void Build(CarController car)
        {
            paint=Material("Prata metálico • carroceria",new Color(.57f,.64f,.68f),.66f,.78f);
            glass=Material("Vidro fumê azulado",new Color(.055f,.12f,.16f),.38f,.94f);
            black=Material("Frisos e borrachas",new Color(.025f,.031f,.038f),.08f,.36f);
            chrome=Material("Alumínio das rodas",new Color(.59f,.65f,.69f),.82f,.76f);
            tire=Material("Borracha dos pneus",new Color(.022f,.025f,.027f),0,.23f);
            lens=Material("Lentes dos faróis",new Color(.79f,.87f,.91f),.1f,.91f);
            gold=Material("Emblema dourado",new Color(.82f,.56f,.15f),.8f,.7f);
            white=Material("Placa branca",new Color(.86f,.9f,.91f),.05f,.4f);
            red=Material("Lente vermelha",new Color(.5f,.025f,.035f),.2f,.8f);
            amber=Material("Lente âmbar",new Color(.8f,.31f,.025f),.1f,.75f);
            body=new GameObject(ModelName).transform;body.SetParent(car.transform,false);
            Shell();Cabin();Details(car);
            var animated=new List<Renderer>();animated.AddRange(car.leftLights);animated.AddRange(car.rightLights);animated.AddRange(car.brakeLights);animated.AddRange(car.reverseLights);CombineStatic(body,animated.ToArray());
        }
        static void Shell()
        {
            // Side surfaces follow circular wheel arches, instead of hiding the tires in a solid cube.
            const int samples=100;
            for(int side=-1;side<=1;side+=2)
            {
                var vertices=new List<Vector3>();var triangles=new List<int>();
                for(int i=0;i<=samples;i++)
                {
                    float z=Mathf.Lerp(-1.85f,1.85f,(float)i/samples),bottom=.33f;
                    foreach(float axle in new[]{-1.14f,1.2f})
                    { float d=Mathf.Abs(z-axle);if(d<.36f)bottom=Mathf.Max(bottom,.4f+Mathf.Sqrt(.36f*.36f-d*d)); }
                    float top=Belt(z);
                    vertices.Add(new Vector3(side*Width(z)*.97f,bottom,z));
                    vertices.Add(new Vector3(side*Width(z),Mathf.Lerp(bottom,top,.72f),z));
                    vertices.Add(new Vector3(side*Width(z)*.96f,top,z));
                }
                for(int i=0;i<samples;i++)for(int j=0;j<2;j++){int a=i*3+j;triangles.AddRange(new[]{a,a+3,a+1,a+1,a+3,a+4});}
                var sideMesh=MeshObject("Lateral com caixas de roda",body,vertices,triangles,paint,true);
                BodyVertexCount=sideMesh.GetComponent<MeshFilter>().sharedMesh.vertexCount;
                // Thin black arch lip gives the wheel well a visible edge.
                foreach(float axle in new[]{-1.14f,1.2f})for(int i=0;i<18;i++)
                {
                    float a=i*Mathf.PI/18,b=(i+1)*Mathf.PI/18;
                    Beam("Borda da caixa de roda",body,new Vector3(side*.815f,.4f+Mathf.Sin(a)*.357f,axle+Mathf.Cos(a)*.357f),new Vector3(side*.815f,.4f+Mathf.Sin(b)*.357f,axle+Mathf.Cos(b)*.357f),.014f,black);
                }
                foreach(float axle in new[]{-1.14f,1.2f})Primitive("Interior escuro da caixa de roda",PrimitiveType.Cylinder,body,new Vector3(side*.69f,.4f,axle),new Vector3(.69f,.015f,.69f),black,Quaternion.Euler(0,0,90));
            }
            Box("Assoalho escuro",body,new Vector3(0,.34f,0),new Vector3(1.43f,.07f,3.35f),black);
            // Curved hood, sampled across both axes to keep highlights continuous.
            var v=new List<Vector3>();var t=new List<int>();const int columns=16,rows=16;
            for(int i=0;i<=rows;i++)for(int j=0;j<=columns;j++)
            {
                float z=Mathf.Lerp(.6f,1.84f,(float)i/rows),u=(float)j/columns*2-1;
                float y=Belt(z)+.035f*(1-u*u);
                v.Add(new Vector3(u*Width(z)*.96f,y,z));
            }
            for(int i=0;i<rows;i++)for(int j=0;j<columns;j++){int a=i*(columns+1)+j;t.AddRange(new[]{a,a+columns+1,a+1,a+1,a+columns+1,a+columns+2});}
            MeshObject("Capô curvo",body,v,t,paint,true);
            // Painted front and rear bumpers taper at their corners.
            foreach(int end in new[]{-1,1})
            {
                var points=new List<Vector3>();var indices=new List<int>();
                for(int i=0;i<=20;i++)
                {
                    float u=i/20f*2-1,z=end*(1.9f-(end==1?.05f:.14f)*Mathf.Pow(Mathf.Abs(u),4));
                    points.Add(new Vector3(u*.78f,.33f+.05f*Mathf.Abs(u),z));
                    points.Add(new Vector3(u*.79f,end==1?.84f:.96f,z));
                }
                for(int i=0;i<20;i++){int a=i*2;indices.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}
                MeshObject(end==1?"Para-choque dianteiro":"Tampa e para-choque traseiro",body,points,indices,paint,true);
            }
            var hatch=new List<Vector3>();var hatchTriangles=new List<int>();
            for(int i=0;i<=20;i++)
            {
                float u=i/20f*2-1;
                hatch.Add(new Vector3(u*.79f,.96f,-1.9f+.14f*Mathf.Pow(Mathf.Abs(u),4)));
                hatch.Add(new Vector3(u*.72f,1f,-1.65f));
            }
            for(int i=0;i<20;i++){int a=i*2;hatchTriangles.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}
            MeshObject("Ombro da tampa traseira",body,hatch,hatchTriangles,paint,true);
        }
        static void Cabin()
        {
            // Rounded roof with distinct raked front and rear glass.
            var v=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<=16;i++)for(int j=0;j<=16;j++)
            {
                float z=Mathf.Lerp(-.96f,.28f,i/16f),u=j/16f*2-1;
                v.Add(new Vector3(u*.665f,1.445f+.055f*(1-u*u)+.015f*Mathf.Sin(i/16f*Mathf.PI),z));
            }
            for(int i=0;i<16;i++)for(int j=0;j<16;j++){int a=i*17+j;t.AddRange(new[]{a,a+17,a+1,a+1,a+17,a+18});}
            MeshObject("Teto arqueado",body,v,t,paint,true);
            Panel("Para-brisa inclinado",body,glass,new Vector3(-.736f,.99f,.87f),new Vector3(.736f,.99f,.87f),new Vector3(.645f,1.44f,.28f),new Vector3(-.645f,1.44f,.28f));
            Panel("Vidro da tampa traseira",body,glass,new Vector3(-.72f,1.0f,-1.65f),new Vector3(-.64f,1.435f,-.97f),new Vector3(.64f,1.435f,-.97f),new Vector3(.72f,1.0f,-1.65f));
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 A=new Vector3(side*.77f,.99f,.78f),B=new Vector3(side*.65f,1.435f,.25f),C=new Vector3(side*.65f,1.435f,-.2f),D=new Vector3(side*.785f,.99f,-.2f);
                Panel("Vidro da porta dianteira",body,glass,A,B,C,D);
                Panel("Vidro da porta traseira",body,glass,new Vector3(side*.782f,.99f,-.3f),new Vector3(side*.65f,1.435f,-.3f),new Vector3(side*.65f,1.435f,-.88f),new Vector3(side*.765f,.99f,-1.49f));
                Beam("Coluna A",body,A,B,.065f,paint);
                Beam("Coluna B",body,new Vector3(side*.79f,.97f,-.25f),new Vector3(side*.66f,1.455f,-.25f),.085f,black);
                Beam("Coluna traseira",body,new Vector3(side*.765f,.965f,-1.59f),new Vector3(side*.66f,1.42f,-.95f),.08f,paint);
                Beam("Friso inferior dos vidros",body,new Vector3(side*.79f,.973f,.78f),new Vector3(side*.79f,.973f,-1.52f),.025f,black);
                foreach(float seam in new[]{-.25f,-1.42f,.79f})Beam("Vão da porta",body,new Vector3(side*(Width(seam)+.003f),.48f,seam),new Vector3(side*(Width(seam)*.97f+.008f),.96f,seam),.008f,black);
                foreach(float handle in new[]{.04f,-.95f})Primitive("Maçaneta preta",PrimitiveType.Sphere,body,new Vector3(side*.814f,.883f,handle),new Vector3(.033f,.045f,.145f),black);
                Box("Friso lateral",body,new Vector3(side*.823f,.6f,.02f),new Vector3(.025f,.055f,1.3f),black);
                Beam("Suporte do retrovisor",body,new Vector3(side*.76f,1.035f,.71f),new Vector3(side*.93f,1.07f,.63f),.06f,black);
                Primitive("Retrovisor arredondado",PrimitiveType.Sphere,body,new Vector3(side*.94f,1.095f,.62f),new Vector3(.25f,.16f,.29f),paint);
                Primitive("Espelho externo",PrimitiveType.Sphere,body,new Vector3(side*.94f,1.095f,.505f),new Vector3(.2f,.115f,.025f),chrome);
            }
            Beam("Limpador dianteiro E",body,new Vector3(-.55f,1.015f,.86f),new Vector3(-.08f,1.035f,.82f),.017f,black);
            Beam("Limpador dianteiro D",body,new Vector3(.05f,1.015f,.86f),new Vector3(.49f,1.035f,.82f),.017f,black);
            Beam("Limpador traseiro",body,new Vector3(.04f,1.025f,-1.63f),new Vector3(.42f,1.09f,-1.54f),.018f,black);
            Beam("Antena",body,new Vector3(0,1.51f,-.72f),new Vector3(0,1.79f,-.95f),.012f,black);
        }
        static void Emblem(Vector3 center,bool front)
        {
            var shape=new[]{new Vector2(-.11f,-.024f),new Vector2(-.042f,-.024f),new Vector2(-.042f,-.049f),new Vector2(.042f,-.049f),new Vector2(.042f,-.024f),new Vector2(.11f,-.024f),new Vector2(.13f,.024f),new Vector2(.042f,.024f),new Vector2(.042f,.049f),new Vector2(-.042f,.049f),new Vector2(-.042f,.024f),new Vector2(-.13f,.024f)};
            var points=new Vector3[shape.Length];for(int i=0;i<shape.Length;i++)points[i]=center+new Vector3(shape[i].x,shape[i].y,0);
            Panel("Emblema gravata estilizado",body,gold,points);
        }
        static void Details(CarController car)
        {
            Box("Entrada de ar inferior",body,new Vector3(0,.5f,1.905f),new Vector3(1.1f,.19f,.025f),black);
            Box("Grade superior",body,new Vector3(0,.805f,1.903f),new Vector3(.7f,.105f,.025f),black);
            for(int i=0;i<5;i++)Box("Lâmina da grade",body,new Vector3(0,.435f+i*.036f,1.925f),new Vector3(.99f,.008f,.015f),chrome);
            Box("Travessa pintada",body,new Vector3(0,.711f,1.925f),new Vector3(.82f,.08f,.025f),paint);Emblem(new Vector3(0,.744f,1.944f),true);
            Emblem(new Vector3(0,.897f,-1.909f),false);
            var left=new List<Renderer>();var right=new List<Renderer>();var brakes=new List<Renderer>();var reverse=new List<Renderer>();
            for(int side=-1;side<=1;side+=2)
            {
                Panel("Moldura do farol",body,black,new Vector3(side*.335f,.727f,1.928f),new Vector3(side*.765f,.686f,1.879f),new Vector3(side*.775f,.875f,1.871f),new Vector3(side*.418f,.925f,1.916f));
                Panel("Farol trapezoidal",body,lens,new Vector3(side*.35f,.74f,1.93f),new Vector3(side*.75f,.7f,1.88f),new Vector3(side*.76f,.86f,1.873f),new Vector3(side*.43f,.91f,1.918f));
                for(int i=0;i<2;i++)Primitive("Refletor do farol",PrimitiveType.Sphere,body,new Vector3(side*(.44f+i*.17f),.795f,1.934f-i*.023f),new Vector3(.115f,.105f,.015f),white);
                var indicators=side==-1?left:right;
                indicators.Add(Box("Seta dianteira",body,new Vector3(side*.735f,.78f,1.886f),new Vector3(.06f,.11f,.018f),new Material(amber)).GetComponent<Renderer>());
                brakes.Add(RearLens(side,0,1,new Material(red),"Lanterna traseira integrada"));
                indicators.Add(RearLens(side,.52f,.73f,new Material(amber),"Seta traseira"));
                reverse.Add(RearLens(side,.04f,.2f,new Material(lens),"Luz de ré"));
            }
            brakes.Add(Box("Terceira luz de freio",body,new Vector3(0,1.415f,-1.01f),new Vector3(.3f,.025f,.025f),new Material(red)).GetComponent<Renderer>());
            foreach(int end in new[]{-1,1})
            {
                Box("Moldura da placa",body,new Vector3(0,.589f,end*1.93f),new Vector3(.49f,.175f,.02f),black);
                Box("Placa de treinamento",body,new Vector3(0,.59f,end*1.944f),new Vector3(.45f,.145f,.012f),white);
                Box("Faixa azul da placa",body,new Vector3(0,.645f,end*1.952f),new Vector3(.45f,.028f,.013f),Material("Azul placa "+end,new Color(.015f,.12f,.38f)));
                Text("CNH0B03",body,new Vector3(0,.575f,end*1.965f),.018f,Color.black,end==1?Quaternion.Euler(0,180,0):Quaternion.identity);
            }
            Text("CELTA",body,new Vector3(-.43f,.885f,-1.916f),.014f,new Color(.8f,.85f,.88f));
            Primitive("Escapamento",PrimitiveType.Cylinder,body,new Vector3(-.49f,.29f,-1.88f),new Vector3(.065f,.1f,.065f),black,Quaternion.Euler(90,0,0));
            car.leftLights=left.ToArray();car.rightLights=right.ToArray();car.brakeLights=brakes.ToArray();
            car.reverseLights=reverse.ToArray();
            car.beams=new Light[2];
            for(int i=0;i<2;i++)
            {
                var light=new GameObject("Farol baixo").AddComponent<Light>();light.transform.SetParent(body,false);light.transform.localPosition=new Vector3(i==0?-.55f:.55f,.8f,1.96f);light.transform.localRotation=Quaternion.Euler(8,0,0);
                light.type=LightType.Spot;light.range=35;light.spotAngle=58;light.intensity=2.5f;light.color=new Color(1,.94f,.8f);light.enabled=false;car.beams[i]=light;
            }
        }
        static Renderer RearLens(int side,float start,float end,Material material,string name)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<=24;i++)
            {
                float t=Mathf.Lerp(start,end,i/24f),y=Mathf.Lerp(.78f,1.29f,t);
                foreach(float x in new[]{.65f,Mathf.Lerp(.79f,.72f,t)})
                {
                    float lower=-1.9f+.14f*Mathf.Pow(x/.79f,4),z;
                    if(y<=.96f)z=lower;
                    else if(y<1)z=Mathf.Lerp(lower,-1.65f,(y-.96f)/.04f);
                    else z=Mathf.Lerp(-1.65f,-.97f,(y-1)/.435f);
                    vertices.Add(new Vector3(side*x,y,z-.019f-(name=="Lanterna traseira integrada"?0:.008f)));
                }
            }
            for(int i=0;i<24;i++){int a=i*2;triangles.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}
            return MeshObject(name,body,vertices,triangles,material,true).GetComponent<Renderer>();
        }
        public static Transform Wheel(Transform parent,int index)
        {
            var root=new GameObject("Roda detalhada "+index).transform;root.SetParent(parent,false);
            Lathe("Pneu com perfil arredondado",root,new[]{new Vector2(.19f,-.105f),new Vector2(.26f,-.105f),new Vector2(.285f,-.075f),new Vector2(.292f,-.04f),new Vector2(.292f,.04f),new Vector2(.285f,.075f),new Vector2(.26f,.105f),new Vector2(.19f,.105f)},tire,48);
            foreach(int side in new[]{-1,1})
            {
                Lathe("Aro de alumínio",root,new[]{new Vector2(.17f,side*.11f),new Vector2(.19f,side*.11f),new Vector2(.19f,side*.118f),new Vector2(.17f,side*.118f)},chrome);
                Primitive("Cubo da roda",PrimitiveType.Cylinder,root,new Vector3(0,side*.104f,0),new Vector3(.115f,.02f,.115f),chrome);
                for(int i=0;i<5;i++)
                {
                    float angle=i*Mathf.PI*2/5;
                    Beam("Raio da roda",root,new Vector3(Mathf.Sin(angle)*.055f,side*.115f,Mathf.Cos(angle)*.055f),new Vector3(Mathf.Sin(angle+.17f)*.17f,side*.115f,Mathf.Cos(angle+.17f)*.17f),.037f,chrome);
                    Primitive("Parafuso",PrimitiveType.Sphere,root,new Vector3(Mathf.Sin(angle)*.04f,side*.128f,Mathf.Cos(angle)*.04f),Vector3.one*.013f,black);
                }
            }
            for(int i=0;i<32;i++)
            { float a=i*Mathf.PI*2/32;Box("Sulco da banda de rodagem",root,new Vector3(Mathf.Sin(a)*.2925f,0,Mathf.Cos(a)*.2925f),new Vector3(.009f,.13f,.003f),black,Quaternion.Euler(0,a*Mathf.Rad2Deg,0)); }
            CombineStatic(root);return root;
        }
    }
}
