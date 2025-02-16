using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public abstract class BaseCommandSO : SerializedScriptableObject, ICommand
{
    public abstract Task Execute();
    public bool IsExecuting { get; protected set; }
    [SerializeField] protected bool removeAfterExecute;

    public bool RemoveAfterExecute => removeAfterExecute;

    public static T Create<T>() where T : BaseCommandSO
    {
        return (T)CreateInstance(typeof(T));
    }
}