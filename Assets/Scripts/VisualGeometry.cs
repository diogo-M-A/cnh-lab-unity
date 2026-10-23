using System.Collections.Generic;
using UnityEngine;

namespace CnhTreino
{
    // Render-only geometry. Colliders stay in TrainingWorld so cosmetic changes cannot alter the exam.
    public static class VisualGeometry
    {
        static readonly Dictionary<Font,Material> fontMaterials=new Dictionary<Font,Material>();
        public static Material FontMaterial(Font font)
        {
            if(!fontMaterials.TryGetValue(font,out var material))
            {
                material=new Material(Shader.Find("CNHLab/DepthText")){name="Texto com profundidade",mainTexture=font.material.mainTexture};fontMaterials.Add(font,material);
                Font.textureRebuilt+=updated=>{if(updated==font&&material)material.mainTexture=font.material.mainTexture;};
            }
            material.mainTexture=font.material.mainTexture;return material;
        }
        public static Material Material(string name, Color color, float metallic=0, float smoothness=.3f)
        {
            var material=new Material(Shader.Find("Standard")){name=name,color=color};
            material.SetFloat("_Metallic",metallic);material.SetFloat("_Glossiness",smoothness);return material;
        }
        public static GameObject MeshObject(string name,Transform parent,List<Vector3> vertices,List<int> triangles,Material material,bool doubleSided=false)
        {
            if(doubleSided)
            {
                int count=vertices.Count,indices=triangles.Count;
                vertices.AddRange(vertices.ToArray());
                for(int i=0;i<indices;i+=3){triangles.Add(triangles[i]+count);triangles.Add(triangles[i+2]+count);triangles.Add(triangles[i+1]+count);}
            }
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=material;return obj;
        }
        public static GameObject Panel(string name,Transform parent,Material material,params Vector3[] points)
        {
            var triangles=new List<int>();for(int i=1;i<points.Length-1;i++){triangles.Add(0);triangles.Add(i);triangles.Add(i+1);}
            return MeshObject(name,parent,new List<Vector3>(points),triangles,material,true);
        }
        public static GameObject Primitive(string name,PrimitiveType type,Transform parent,Vector3 position,Vector3 scale,Material material,Quaternion rotation=default)
        {
            var obj=GameObject.CreatePrimitive(type);obj.name=name;obj.transform.SetParent(parent,false);obj.transform.localPosition=position;obj.transform.localScale=scale;
            obj.transform.localRotation=rotation==default?Quaternion.identity:rotation;obj.GetComponent<Renderer>().sharedMaterial=material;Object.Destroy(obj.GetComponent<Collider>());return obj;
        }
        public static GameObject Box(string name,Transform parent,Vector3 p,Vector3 size,Material material,Quaternion rotation=default)
        { return Primitive(name,PrimitiveType.Cube,parent,p,size,material,rotation); }
        public static GameObject Beam(string name,Transform parent,Vector3 from,Vector3 to,float width,Material material)
        { return Box(name,parent,(from+to)*.5f,new Vector3(width,Vector3.Distance(from,to),width),material,Quaternion.FromToRotation(Vector3.up,to-from)); }
        public static GameObject Lathe(string name,Transform parent,Vector2[] profile,Material material,int segments=40)
        {
            var v=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<=segments;i++)
            {
                float a=i*Mathf.PI*2/segments;
                foreach(var p in profile)v.Add(new Vector3(Mathf.Cos(a)*p.x,p.y,Mathf.Sin(a)*p.x));
            }
            for(int i=0;i<segments;i++)for(int j=0;j<profile.Length;j++)
            {
                int a=i*profile.Length+j,b=i*profile.Length+(j+1)%profile.Length,c=a+profile.Length,d=b+profile.Length;
                t.AddRange(new[]{a,b,c,b,d,c});
            }
            return MeshObject(name,parent,v,t,material,true);
        }
        public static GameObject Disc(string name,Transform parent,Vector3 position,float radius,Material material,int sides=48)
        {
            var points=new Vector3[sides];for(int i=0;i<sides;i++){float angle=(i+.5f)*Mathf.PI*2/sides;points[i]=position+new Vector3(Mathf.Sin(angle)*radius,Mathf.Cos(angle)*radius,0);}
            return Panel(name,parent,material,points);
        }
        public static void CombineStatic(Transform root,Renderer[] animated=null)
        {
            var groups=new Dictionary<Material,List<CombineInstance>>();var originals=new List<GameObject>();
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var renderer=filter.GetComponent<Renderer>();
                if(!renderer||!filter.sharedMesh||renderer.sharedMaterial==null)continue;
                if(animated!=null&&System.Array.IndexOf(animated,renderer)>=0)continue;
                if(!groups.TryGetValue(renderer.sharedMaterial,out var group)){group=new List<CombineInstance>();groups.Add(renderer.sharedMaterial,group);}
                group.Add(new CombineInstance{mesh=filter.sharedMesh,transform=root.worldToLocalMatrix*filter.transform.localToWorldMatrix});originals.Add(filter.gameObject);
            }
            foreach(var entry in groups)
            {
                var mesh=new Mesh{name="Geometria combinada • "+entry.Key.name};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.CombineMeshes(entry.Value.ToArray(),true,true);
                var obj=new GameObject(mesh.name);obj.transform.SetParent(root,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=entry.Key;
            }
            foreach(var obj in originals){obj.SetActive(false);Object.Destroy(obj);}
        }
        public static void Text(string text,Transform parent,Vector3 p,float size,Color color,Quaternion rotation=default)
        {
            var obj=new GameObject(text);obj.transform.SetParent(parent,false);obj.transform.localPosition=p;obj.transform.localRotation=rotation==default?Quaternion.identity:rotation;
            var tm=obj.AddComponent<TextMesh>();tm.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");tm.GetComponent<Renderer>().sharedMaterial=FontMaterial(tm.font);
            tm.text=text;tm.fontSize=64;tm.characterSize=size;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.color=color;
        }
    }
}
