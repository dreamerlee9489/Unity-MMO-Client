using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace App
{
    public class TestTask : MonoBehaviour
	{
		CancellationTokenSource tokenSource;

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.W)) 
            {
                tokenSource.Cancel();
            }

            if(Input.GetKeyDown(KeyCode.Q))
            {
                tokenSource = new();
                Task.Run(Test, tokenSource.Token);
            }
        }

        private void Test()
        {
            while (!tokenSource.IsCancellationRequested)
            {
                print("Running");
                Thread.Sleep(1000);
            }
            print("IsCancellationRequested: " + tokenSource.IsCancellationRequested);
        }
    }
}
