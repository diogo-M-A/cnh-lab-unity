using System;
using System.Reflection;
using UnityEngine;

namespace CnhTreino
{
    public static class ScenarioChecks
    {
        static void Invoke(TrainingSession s,string method,Vector3 p)
        { typeof(TrainingSession).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(s,new object[]{p}); }
        static void Field(TrainingSession s,string field,object value)
        { typeof(TrainingSession).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(s,value); }
        static void Require(bool ok,string name){if(!ok)throw new Exception(name);Debug.Log("SCENARIO PASS: "+name);}
        public static bool Run(TrainingSession s)
        {
            try
            {
                s.enabled=false;
                s.Reset(0);Invoke(s,"Urban",new Vector3(2.8f,0,23));Require(s.points==6,"PARE sem parada gera ocorrência");
                s.Reset(0);Field(s,"stopSatisfied",true);Invoke(s,"Urban",new Vector3(2.8f,0,23));Require(s.points==0,"PARE com parada aceita");
                s.Reset(0);Field(s,"stoppedAtSign",1.9f);s.Car.body.linearVelocity=Vector3.forward;Invoke(s,"Urban",new Vector3(2.8f,0,20));s.Car.body.linearVelocity=Vector3.zero;Invoke(s,"Urban",new Vector3(2.8f,0,23));Require(s.points==6,"PARE não soma pausas interrompidas");
                s.Reset(0);s.Car.body.linearVelocity=Vector3.down*4;Require(s.Car.Kph<.01f,"queda vertical não conta como direção");s.Car.body.linearVelocity=Vector3.zero;
                s.Reset(0);Field(s,"previous",new Vector3(2.8f,0,58));s.Car.signal=0;Invoke(s,"Urban",new Vector3(2.8f,0,62));Require(s.points==6,"conversão sem seta e observação");
                s.Reset(0);Field(s,"previous",new Vector3(2.8f,0,58));s.Car.signal=1;s.Car.Observe();Invoke(s,"Urban",new Vector3(2.8f,0,62));Require(s.points==0&&s.stage==1,"conversão sinalizada e observada");
                s.Reset(1);Field(s,"parkTime",2.1f);s.Car.body.rotation=Quaternion.Euler(0,30,0);s.Car.transform.rotation=Quaternion.Euler(0,30,0);Invoke(s,"Parking",new Vector3(8,0,12));Require(!s.complete,"vaga recusa desalinhamento");
                s.Car.body.rotation=Quaternion.identity;s.Car.transform.rotation=Quaternion.identity;Field(s,"parkTime",2.1f);Invoke(s,"Parking",new Vector3(8,0,12));Require(s.complete,"vaga alinhada e segura conclui");
                s.Reset(2);Field(s,"hillStop",3.1f);Invoke(s,"Hill",new Vector3(-38,0,15));Require(!s.complete,"parada na rampa exige saída");
                Invoke(s,"Hill",new Vector3(-38,0,14));Require(s.points==2,"recuo na rampa detectado");
                Invoke(s,"Hill",new Vector3(-38,0,32));Require(s.complete,"rampa conclui no topo");
                s.Reset(2);Field(s,"hillHeld",true);Invoke(s,"Hill",new Vector3(0,0,32));Require(!s.complete,"rampa recusa conclusão fora da pista");
                s.Reset(0);s.stage=6;s.Car.motor.running=false;Invoke(s,"Urban",s.Target);Require(s.complete,"finalização urbana segura");
                Require(s.Car.paused&&Time.timeScale==0,"conclusão interrompe condução");
                s.Reset(1);Require(!s.Car.paused&&Time.timeScale==1&&!s.complete,"reinício retoma física após conclusão");
                Require(s.Car.headlights==false&&s.Car.wheels[0].motorTorque==0,"reinício limpa comandos do veículo");
                return true;
            }
            catch(Exception e){Debug.LogError("Scenario FAIL: "+e);return false;}
        }
    }
}
