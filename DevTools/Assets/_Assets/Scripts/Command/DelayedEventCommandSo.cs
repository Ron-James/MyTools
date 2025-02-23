using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "DelayedEventCommand", menuName = "Command/Delayed Event Command")]
public class DelayedEventCommandSo<T> : BaseCommandSO
{
    [SerializeField] private CancellationTokenSource _cancellationTokenSource;
    [SerializeField] private IEvent<T> _event;
    [SerializeField] T _value;
    [SerializeField] private float _delay;

    public float Delay => _delay;


    public override async Task Execute()
    {
        //If the token source is null, create a new one
        if (_cancellationTokenSource == null)
            _cancellationTokenSource = new CancellationTokenSource();


        IsExecuting = true;
        await Awaitable.WaitForSecondsAsync(Delay, _cancellationTokenSource.Token);
        _event.Raise(_value);
        IsExecuting = false;
    }
}