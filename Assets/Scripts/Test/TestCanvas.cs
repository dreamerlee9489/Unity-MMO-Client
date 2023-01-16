using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace App
{
	public class TestCanvas : MonoBehaviour
	{
		public InputField inputField;

        private void Start()
        {
            inputField.onEndEdit.AddListener((str) =>
            {
                print("onEndEdit " + str); 
            });
            inputField.onValueChanged.AddListener((str) => 
            {
                print("onValueChanged " + str);
            });
            inputField.onSubmit.AddListener((str) => 
            {
                print("onSubmit " + str);
            });
        }
    }
}
