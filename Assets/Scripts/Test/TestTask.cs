using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace App
{
	public class TestTask : MonoBehaviour
	{
		CancellationTokenSource tokenSource = new();

        private void Awake()
        {
            tokenSource.Token.Register(() => { print("取消Task回调"); });
            Task.Run(() =>
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    print("Running");
                    Thread.Sleep(1000);
                }
                print("IsCancellationRequested: " + tokenSource.IsCancellationRequested);
            });
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape)) 
            {
                tokenSource.Cancel();
            }
        }
    }
}
