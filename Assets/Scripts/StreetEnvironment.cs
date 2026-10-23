using UnityEngine;
using UnityEngine.Rendering;
using static CnhTreino.VisualGeometry;

namespace CnhTreino
{
    public static class StreetEnvironment
    {
        static Texture2D leafTexture;
        public static void Lighting(Light sun)
        {
            QualitySettings.antiAliasing=4;QualitySettings.shadowDistance=90;QualitySettings.shadowResolution=ShadowResolution.High;
            QualitySettings.shadowCascades=2;QualitySettings.pixelLightCount=4;QualitySettings.anisotropicFiltering=AnisotropicFiltering.Enable;
            sun.color=new Color(1,.92f,.81f);sun.intensity=1.22f;sun.shadowStrength=.82f;sun.shadowBias=.035f;sun.transform.rotation=Quaternion.Euler(36,-32,0);RenderSettings.sun=sun;
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.56f,.66f,.79f);RenderSettings.ambientEquatorColor=new Color(.39f,.43f,.46f);RenderSettings.ambientGroundColor=new Color(.24f,.25f,.22f);
            var shader=Shader.Find("Skybox/Procedural");
            if(shader)
            {
                var sky=new Material(shader);sky.SetColor("_SkyTint",new Color(.48f,.59f,.72f));sky.SetColor("_GroundColor",new Color(.32f,.36f,.31f));
                sky.SetFloat("_AtmosphereThickness",.85f);sky.SetFloat("_SunSize",.025f);sky.SetFloat("_Exposure",1.05f);RenderSettings.skybox=sky;
            }
            // A small static environment map gives the metallic body and windows reflections without per-frame probe rendering.
            var cube=new Cubemap(64,TextureFormat.RGB24,false);cube.name="Reflexo procedural de céu e horizonte";
            for(int face=0;face<6;face++)
            {
                var colors=new Color[64*64];
                for(int y=0;y<64;y++)for(int x=0;x<64;x++)
                {
                    float u=x/63f*2-1,v=y/63f*2-1;Vector3 d;
                    switch(face){case 0:d=new Vector3(1,-v,-u);break;case 1:d=new Vector3(-1,-v,u);break;case 2:d=new Vector3(u,1,v);break;case 3:d=new Vector3(u,-1,-v);break;case 4:d=new Vector3(u,-v,1);break;default:d=new Vector3(-u,-v,-1);break;}
                    d.Normalize();Color c=d.y>0?Color.Lerp(new Color(.82f,.84f,.82f),new Color(.27f,.45f,.68f),Mathf.Sqrt(d.y)):Color.Lerp(new Color(.36f,.39f,.33f),new Color(.12f,.14f,.12f),-d.y);
                    c+=new Color(1,.9f,.7f)*Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,-sun.transform.forward)),180)*1.5f;
                    colors[y*64+x]=c;
                }
                cube.SetPixels(colors,(CubemapFace)face);
            }
            cube.Apply();RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;RenderSettings.customReflectionTexture=cube;RenderSettings.reflectionIntensity=.8f;
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.68f,.76f,.8f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.003f;
        }
        public static Texture2D Grain(string name,int size,int seed)
        {
            var texture=new Texture2D(size,size,TextureFormat.RGB24,true);texture.name=name;texture.wrapMode=TextureWrapMode.Repeat;texture.filterMode=FilterMode.Trilinear;texture.anisoLevel=4;
            var colors=new Color[size*size];var random=new System.Random(seed);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            { float value=.7f+(float)random.NextDouble()*.35f+Mathf.PerlinNoise(x*.035f,y*.035f)*.15f;colors[y*size+x]=new Color(value,value,value); }
            texture.SetPixels(colors);texture.Apply();return texture;
        }
        public static void Tree(Vector3 position,Transform parent)
        {
            int seed=Mathf.Abs(Mathf.RoundToInt(position.x*9+position.z*7));float size=.9f+(seed%7)*.04f;
            var tree=new GameObject("Árvore de rua").transform;tree.SetParent(parent,false);tree.localPosition=position;
            var bark=Material("Casca",new Color(.28f,.2f,.13f),0,.2f);
            Primitive("Tronco",PrimitiveType.Cylinder,tree,new Vector3(0,1.25f,0),new Vector3(.27f,1.25f,.27f),bark);
            for(int i=0;i<5;i++)
            {
                float angle=i*Mathf.PI*2/5;Vector3 crown=new Vector3(Mathf.Cos(angle)*.7f,2.7f+(i%3)*.35f,Mathf.Sin(angle)*.7f);
                Beam("Galho",tree,new Vector3(0,1.6f,0),crown,.09f,bark);
                var leaf=Material("Folhas",Color.Lerp(new Color(.13f,.26f,.095f),new Color(.28f,.43f,.15f),(i+seed%3)/7f),0,.27f);
                if(!leafTexture)leafTexture=Grain("Textura das folhas",128,9);leaf.mainTexture=leafTexture;leaf.mainTextureScale=Vector2.one*3;
                var foliage=Primitive("Copa irregular",PrimitiveType.Sphere,tree,crown,new Vector3(2.1f,2.4f,2)*size,leaf);
                var mesh=foliage.GetComponent<MeshFilter>().mesh;var vertices=mesh.vertices;
                for(int j=0;j<vertices.Length;j++){var p=vertices[j];vertices[j]*=1+.11f*Mathf.Sin(p.x*29+seed)*Mathf.Cos(p.y*23+p.z*19);}
                mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();
            }
        }
        public static void Build(TrainingWorld world)
        {
            var root=new GameObject("Bairro • acabamento visual").transform;
            var sidewalk=Material("Concreto das calçadas",new Color(.57f,.56f,.5f),0,.35f);sidewalk.mainTexture=Grain("Concreto granulado",128,4);
            var dark=Material("Ferro pintado",new Color(.08f,.1f,.105f),.35f,.4f);
            var wood=Material("Madeira dos bancos",new Color(.34f,.19f,.09f),0,.35f);
            foreach(float x in new[]{-8.1f,68.1f})
            {
                Box("Calçada externa",root,new Vector3(x,.1f,35),new Vector3(3.2f,.13f,54),sidewalk);
                for(int z=9;z<63;z+=2)Box("Junta do passeio",root,new Vector3(x,.17f,z),new Vector3(3.1f,.006f,.012f),dark);
                for(int z=12;z<61;z+=16)Lamp(root,new Vector3(x,.17f,z),dark);
            }
            Box("Passeio junto à baliza",root,new Vector3(11.1f,.1f,12),new Vector3(1.4f,.15f,17),sidewalk);
            Box("Passeio interno",root,new Vector3(8.1f,.1f,42),new Vector3(3.2f,.13f,30),sidewalk);
            // Preserve the parking access, route gates, stop lines and hill collision surfaces.
            for(int i=0;i<4;i++)
            {
                Vector3 p=new Vector3(15+i*10,0,48);
                Box("Assento do banco",root,p+new Vector3(0,.5f,0),new Vector3(2,.12f,.65f),wood);
                Box("Encosto do banco",root,p+new Vector3(0,.86f,.3f),new Vector3(2,.48f,.08f),wood,Quaternion.Euler(-8,0,0));
                for(int side=-1;side<=1;side+=2)Box("Pé do banco",root,p+new Vector3(side*.72f,.25f,0),new Vector3(.1f,.5f,.5f),dark);
            }
            Box("Caminho do jardim",root,new Vector3(30,.01f,35),new Vector3(2,.08f,32),sidewalk);
            var wall=Material("Muro rebocado",new Color(.76f,.72f,.63f),0,.3f);
            var window=Material("Vidros das casas",new Color(.12f,.22f,.27f),.35f,.85f);
            var roof=Material("Telhas terracota",new Color(.42f,.19f,.105f),0,.25f);
            for(int i=0;i<8;i++)
            {
                float x=2+i*9,z=92,h=3.8f+(i%3)*1.25f;
                var facade=Material("Fachada "+i,Color.Lerp(new Color(.82f,.8f,.7f),new Color(.58f,.67f,.63f),i%4/4f),0,.35f);
                Box("Casa",root,new Vector3(x,h*.5f,z),new Vector3(7,h,8),facade);
                Panel("Água frontal do telhado",root,roof,new Vector3(x-4,h,87.4f),new Vector3(x+4,h,87.4f),new Vector3(x+4,h+1.15f,92),new Vector3(x-4,h+1.15f,92));
                Panel("Água traseira do telhado",root,roof,new Vector3(x-4,h+1.15f,92),new Vector3(x+4,h+1.15f,92),new Vector3(x+4,h,96.6f),new Vector3(x-4,h,96.6f));
                for(int tile=0;tile<16;tile++)Beam("Linha das telhas",root,new Vector3(x-3.9f+tile*.5f,h+.02f,87.4f),new Vector3(x-3.9f+tile*.5f,h+1.17f,92),.025f,roof);
                for(int floor=0;floor<(h>5?2:1);floor++)for(int side=-1;side<=1;side+=2)
                {
                    Vector3 p=new Vector3(x+side*1.9f,1.8f+floor*2.3f,87.97f);
                    Box("Moldura de janela",root,p,new Vector3(1.55f,1.4f,.12f),wall);
                    Box("Janela",root,p+Vector3.back*.08f,new Vector3(1.3f,1.15f,.04f),window);
                    Box("Montante da janela",root,p+Vector3.back*.11f,new Vector3(.045f,1.14f,.04f),wall);
                }
                Box("Porta",root,new Vector3(x,1.1f,87.92f),new Vector3(.95f,2.2f,.12f),wood);
                Box("Muro de jardim",root,new Vector3(x,.52f,84.5f),new Vector3(7.5f,1.04f,.23f),wall);
                for(int fence=0;fence<14;fence++)Box("Grade do jardim",root,new Vector3(x-3.4f+fence*.52f,1.3f,84.5f),new Vector3(.035f,.65f,.035f),dark);
            }
            // Low distant hills close the horizon without blocking the training area.
            for(int i=0;i<5;i++)
            {
                float x=-5+i*11,z=-28,h=3.6f;
                Box("Casa ao sul",root,new Vector3(x,h/2,z),new Vector3(8,h,7),wall);
                Panel("Telhado ao sul",root,roof,new Vector3(x-4.5f,h,z-4),new Vector3(x+4.5f,h,z-4),new Vector3(x+4.5f,h+1.2f,z),new Vector3(x-4.5f,h+1.2f,z));
                Panel("Telhado ao sul • frente",root,roof,new Vector3(x-4.5f,h+1.2f,z),new Vector3(x+4.5f,h+1.2f,z),new Vector3(x+4.5f,h,z+4),new Vector3(x-4.5f,h,z+4));
                for(int side=-1;side<=1;side+=2)Box("Janela ao sul",root,new Vector3(x+side*2,1.9f,z+3.51f),new Vector3(1.5f,1.25f,.06f),window);
            }
            var hill=Material("Vegetação distante",new Color(.24f,.34f,.21f),0,.2f);
            for(int i=0;i<9;i++)Primitive("Colina distante",PrimitiveType.Sphere,root,new Vector3(-120+i*35,-4,153+(i%3)*8),new Vector3(75,25+i%3*9,60),hill);
            for(int i=0;i<10;i++)
            {
                float x=14+(i%5)*9,z=21+(i/5)*16;
                var leaves=Material("Arbusto",new Color(.2f,.34f,.14f),0,.3f);
                Primitive("Arbusto do jardim",PrimitiveType.Sphere,root,new Vector3(x,.45f,z),new Vector3(2.8f,1.1f,1.8f),leaves);
            }
        }
        static void Lamp(Transform parent,Vector3 p,Material metal)
        {
            Primitive("Poste de iluminação",PrimitiveType.Cylinder,parent,p+Vector3.up*2.8f,new Vector3(.1f,2.8f,.1f),metal);
            Beam("Braço da luminária",parent,p+new Vector3(0,5.5f,0),p+new Vector3(.85f,5.65f,0),.07f,metal);
            Primitive("Luminária",PrimitiveType.Sphere,parent,p+new Vector3(.85f,5.62f,0),new Vector3(.75f,.12f,.3f),metal);
        }
    }
}
