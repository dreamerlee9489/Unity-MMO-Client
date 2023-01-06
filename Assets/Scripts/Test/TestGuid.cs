using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
	public class TestGuid : MonoBehaviour
	{
		Dictionary<int, int> dict = new();

        private void Start()
        {
			for (int i = 0; i < 1000000; i++)
			{
				int id = Guid.NewGuid().GetHashCode();
				if (!dict.ContainsKey(id))
					dict.Add(id, id);
				else
					Debug.Log("重复");
			}
        }
    }
}
