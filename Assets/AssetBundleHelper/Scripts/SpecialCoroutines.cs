using UnityEngine;
using System;
using System.Collections;

/* 	Coroutine functionality extension. Generic coroutines capable of returning values, being cancelled, and capturing exceptions.
	General usage pattern is:
	Coroutine<Foo> getFoo = StartCoroutine<Foo>(GetFoo())
	yield return getFoo.coroutine;
	Foo foo = getFoo.Value;
	
	Note that if the type parameter derives from YieldInstruction you will not get a return value,
	since those are executed rather than returned in order to allow for coroutine chaining.
*/
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
			if(yielded != null && yielded is T && !(yielded is YieldInstruction)){
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
