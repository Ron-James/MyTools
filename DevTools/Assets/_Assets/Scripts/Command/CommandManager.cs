using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using static System.Activator;


public interface ICommand
{
    Task Execute();
    public bool IsExecuting { get; }
    public bool RemoveAfterExecute { get; }
}

[ShowOdinSerializedPropertiesInInspector]
[Serializable]
public abstract class BaseCommand : ICommand
{
    [SerializeField, ReadOnly] protected bool isExecuting;
    [SerializeField] protected bool removeAfterExecute;
    public abstract Task Execute();

    public bool IsExecuting
    {
        get => isExecuting;
        protected set => value = IsExecuting;
    }

    public bool RemoveAfterExecute => removeAfterExecute;
}

[ShowOdinSerializedPropertiesInInspector]
[Serializable]
public class MoveCommand : BaseCommand
{
    private Vector3 _direction;
    private Transform _transform;
    private float _speed;
    [SerializeField] private float duration = 0;

    public MoveCommand(Transform transform, Vector3 direction, float speed)
    {
        _transform = transform;
        _direction = direction;
        _speed = speed;
    }

    public MoveCommand()
    {
        
    }

    public override async Task Execute()
    {
        IsExecuting = true;
        _transform.position += _direction * _speed * Time.deltaTime;
        await Awaitable.WaitForSecondsAsync(duration);
        IsExecuting = false;
    }
}


public class CommandManager : SerializedMonoBehaviour
{
    [SerializeReference, TableList] Dictionary<UnityEngine.Object, ICommand> _commands = new();


    private CommandInvoker _invoker = new CommandInvoker();

    private void Awake()
    {
        _invoker = new CommandInvoker();
    }

    public async Task ExecuteCommands()
    {
        await _invoker.InvokeSequentially(_commands.Values.ToList());
    }

    public async Task ExecuteCommandsSimultaneously()
    {
        await _invoker.InvokeSimultaneously(_commands.Values.ToList());
    }

    public async Task ExecuteObjectCommandsSequentially(UnityEngine.Object obj)
    {
        List<ICommand> objectCommands = _commands.Where(x => x.Key == obj).Select(x => x.Value).ToList();
        await _invoker.InvokeSequentially(objectCommands);
    }

    public async Task ExecuteObjectCommandsSimultaneously(UnityEngine.Object obj)
    {
        List<ICommand> objectCommands = _commands.Where(x => x.Key == obj).Select(x => x.Value).ToList();
        await _invoker.InvokeSimultaneously(objectCommands);
    }

    public async Task ExecuteSimultaneously(List<ICommand> commands)
    {
        await _invoker.InvokeSimultaneously(commands);
    }


    public async Task ExecuteSingle(ICommand command)
    {
        await _invoker.ExecuteCommand(command);
    }

    [Serializable]
    private class CommandInvoker
    {
        public async Task ExecuteCommand(ICommand command)
        {
            try
            {
                await command.Execute();
            }
            catch (OperationCanceledException e)
            {
#if UNITY_EDITOR
                    Debug.Log(e.Message);
#endif
            }
        }

        public async Task InvokeSimultaneously(List<ICommand> commands)
        {
            var tasks = new List<Task>();
            foreach (var command in commands)
            {
                tasks.Add(ExecuteCommand(command));
            }

            await Task.WhenAll(tasks);
        }

        public async Task InvokeSequentially(List<ICommand> commands)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                await commands[i].Execute();
                if (commands[i].RemoveAfterExecute)
                {
                    commands.RemoveAt(i);
                }
            }
        }

        public async Task InvokeReverse(List<ICommand> commands)
        {
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                try
                {
                    await commands[i].Execute();
                    if (commands[i].RemoveAfterExecute)
                    {
                        commands.RemoveAt(i);
                    }
                }
                catch (OperationCanceledException e)
                {
#if UNITY_EDITOR
                        Debug.Log(e.Message);
#endif
                }
            }
        }
    }
}