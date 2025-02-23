using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "EventCommandSO", menuName = "Command Scriptable Object/EventCommandSO")]
public class DelayedVoidEventCommandSo : BaseCommandSO
{
    [SerializeField] private CancellationTokenSource _cancellationTokenSource;
    [SerializeField] private IEvent _event;
    [SerializeField] private float _delay;
    public override async Task Execute()
    {
        //If the token source is null, create a new one
        if (_cancellationTokenSource == null)
            _cancellationTokenSource = new CancellationTokenSource();

        IsExecuting = true;
        await Awaitable.WaitForSecondsAsync(_delay, _cancellationTokenSource.Token);
        _event.Raise();
        IsExecuting = false;
    }
}