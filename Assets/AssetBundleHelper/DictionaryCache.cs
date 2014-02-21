using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DictionaryCache<T> where T : class {

	private class CachedEntity<V> where V : class{
		public int loadCount;
		public V entity;
		
		public CachedEntity(V entity){
			this.entity = entity;
			loadCount = 0;
		}
	}

	private Dictionary<string, CachedEntity<T>> cache;

	public DictionaryCache(){
		cache = new Dictionary<string, CachedEntity<T>>();
	}

	public bool ContainsValue(T entity){
		return cache.Any(x => x.Value.entity == entity);
	}

	public bool ContainsKey(string id){
		return cache.ContainsKey(id);
	}

	public void Add(string id, T entity){
		if (entity != null){
			cache.Add(id, new CachedEntity<T>(entity));
		}
	}

	public bool Remove(string id) {
		if (cache.ContainsKey(id))
		{
			cache.Remove(id);
			return true;
		}
		return false;
	}

	public bool RemoveValue(T entity) {
		if (ContainsValue(entity))
		{
			var id = cache.First(kvp => (T)kvp.Value.entity == (T)entity).Key;
			cache.Remove(id);
			return true;
		}
		return false;
	}

	public void RemoveUnused(){
		foreach(string id in cache.Keys.Where(key => cache[key].loadCount <= 0).ToList()){
			cache.Remove(id);
		}
	}

	//Calls to Get must be matched by calls to Release
	public T Get(string id) {
		try
		{
			cache[id].loadCount++;
			return cache[id].entity;
		}
		catch (System.Exception)
		{
			return default(T);
		}
	}

	//Get without incrementing loadcount
	public T GetUntracked(string id){
		try{
			return cache[id].entity;
		}
		catch (System.Exception){
			return default(T);
		}
	}

	//Returns whether ID was removed from cache
	public bool Release(string id, bool autoUncache = true){
		CachedEntity<T> cached = cache[id];
		cached.loadCount--;
		if(cached.loadCount == 0 && autoUncache){
			cache.Remove(id);
			return true;
		}
		if(cached.loadCount < 0){
			Debug.LogWarning("ID " + id + " called with Free() more than Get() (" + cached.loadCount +")");
		}
		return false;
	}
}
