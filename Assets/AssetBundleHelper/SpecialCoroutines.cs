using UnityEngine;
using System;
using System.Collections;

public static class MonoBehaviourExt{
	public static Coroutine<T> StartCoroutine<T>(this MonoBehaviour obj, IEnumerator coroutine){
		Coroutine<T> coroutineObject = new Coroutine<T>();
		coroutineObject.coroutine = obj.StartCoroutine(coroutineObject.InternalRoutine(coroutine));
		return coroutineObject;
	}
	
	public static IEnumerator OverTime(
		this MonoBehaviour obj,
		float time, 
	    Func<float, float> f,
		Action<float> action){
		
		float startTime = Time.time;
		while(Time.time - startTime < time){
			float u = f((Time.time - startTime)/time);
			action(u);
			yield return null;
		}
		action(f(1));
		yield break;
	}
}

public class Coroutine<T>{
	public T Value {
		get{
			if(e != null){
				throw e;
			}
			return returnVal;
		}
	}
	
	public void Cancel(){
		isCancelled = true;	
	}
	
	private bool isCancelled = false;
	private T returnVal;
	private Exception e;
	public Coroutine coroutine;
	
	public IEnumerator InternalRoutine(IEnumerator coroutine){
		while(true){
			if(isCancelled){
				e = new CoroutineCancelledException();
				yield break;
			}
			try{
				if(!coroutine.MoveNext()){
					yield break;
				}
			}
			catch(Exception e){
				this.e = e;
				yield break;
			}
			object yielded = coroutine.Current;
			if(yielded != null && yielded is T){
				returnVal = (T)yielded;
				yield break;
			}
			else{
				yield return coroutine.Current;
			}
		}
	}
}

public class CoroutineCancelledException: System.Exception{
	public CoroutineCancelledException():base("Coroutine was cancelled"){
		
	}
}
