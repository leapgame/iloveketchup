

using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// PlayerPrefsの便利クラス
/// </summary>
public static class PlayerPrefsUtility{

	//=================================================================================
	//各型毎の保存
	//=================================================================================

	public static void Save(string key, int value){
		PlayerPrefs.SetInt (key, value);
	}

	public static void Save(string key, float value){
		PlayerPrefs.SetFloat (key, value);
	}

	public static void Save(string key, string value){
		PlayerPrefs.SetString (key, value);
	}

	// public static void Save(string key, decimal value){
	// 	//保存用のメソッドがないのでstringで保存
	// 	PlayerPrefs.SetString (key, value);
	// }

	public static void Save(string key, DateTime value){
		//保存用のメソッドがないのでstringで保存
		PlayerPrefs.SetString (key, value.ToBinary().ToString());
	}

	public static void Save(string key, bool value){
		//そのままでは保存出来ないので指定したkeyが空かどうかで判断
		if(value){
			PlayerPrefs.SetString (key, key);
		}
		else{
			PlayerPrefs.SetString (key, "");
		}
	}

	public static void SaveList<Value>(string key, List<Value> value){
		PlayerPrefs.SetString (key, Serialize<List<Value>> (value));
	}

	public static void SaveDict<Key, Value>(string key, Dictionary<Key, Value> value){
		PlayerPrefs.SetString (key,  Serialize<Dictionary<Key, Value>> (value));
	}

	//=================================================================================
	//各型毎の読み込み
	//=================================================================================

	public static int Load(string key, int defaultValue){
		if(!PlayerPrefs.HasKey (key)){
			return defaultValue;
		}

		return PlayerPrefs.GetInt(key);
	}

	public static float Load(string key, float defaultValue){
		if(!PlayerPrefs.HasKey (key)){
			return defaultValue;
		}

		return PlayerPrefs.GetFloat(key);
	}

	public static string Load(string key, string defaultValue){
		if(!PlayerPrefs.HasKey (key)){
			return defaultValue;
		}

		return PlayerPrefs.GetString(key);
	}

	public static decimal Load(string key, decimal defaultValue){
		if(!PlayerPrefs.HasKey (key)){
			return defaultValue;
		}

		return decimal.Parse (PlayerPrefs.GetString (key));
	}

	public static DateTime Load(string key, DateTime defaultValue){
		if(!PlayerPrefs.HasKey (key)){
			return defaultValue;
		}

		return DateTime.FromBinary (System.Convert.ToInt64 (PlayerPrefs.GetString (key)));
	}

	// public static bool Load(string key, bool defaultValue){
	// 	if(!PlayerPrefs.HasKey (key)){
	// 		return defaultValue;
	// 	}
	//
	// 	return !PlayerPrefs.GetString (key).IsNullOrEmpty();
	// }

	public static List<Value> LoadList<Value> (string key){
		if (!PlayerPrefs.HasKey (key)) {
			return new List<Value> ();
		}

		return Deserialize<List<Value>> (PlayerPrefs.GetString(key));
	}

	public static Dictionary<Key, Value> LoadDict<Key, Value> (string key){
		if (!PlayerPrefs.HasKey (key)) {
			return new Dictionary<Key, Value> ();
		}

		return Deserialize<Dictionary<Key, Value>> (PlayerPrefs.GetString(key));
	}

	//=================================================================================
	//シリアライズ、デシリアライズ
	//=================================================================================

	private static string Serialize<ObjectType> (ObjectType obj){
		BinaryFormatter binaryFormatter = new BinaryFormatter ();
		MemoryStream    memoryStream    = new MemoryStream ();
		binaryFormatter.Serialize (memoryStream , obj);
		return Convert.ToBase64String (memoryStream   .GetBuffer ());
	}

	private static ObjectType Deserialize<ObjectType> (string str){
		BinaryFormatter binaryFormatter = new BinaryFormatter ();
		MemoryStream    memoryStream    = new MemoryStream (Convert.FromBase64String (str));
		return (ObjectType)binaryFormatter.Deserialize (memoryStream);
	}
}