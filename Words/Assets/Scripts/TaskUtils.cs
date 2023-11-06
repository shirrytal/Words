using System.Threading.Tasks;
using Firebase.Extensions;

public class TaskUtils
{
	public delegate void RunOnMainThreadTask();
	public static void RunOnMainThread(RunOnMainThreadTask task) {
        TaskExtension.ContinueWithOnMainThread(Task.CompletedTask, (_) => { task.Invoke(); });
    }
}

