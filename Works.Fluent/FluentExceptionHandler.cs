﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Works.Fluent.Helpers;

namespace Works.Fluent
{
  public class FluentExceptionHandler : FluentInterface
  {
    #region Entry Points

    public static IHandleExceptionOrTry Handle<T>(Action<T> Handler) where T : Exception
    {
      var actualHandler = new FluentExceptionHandlerImpl();
      return actualHandler.Handle(Handler);
    }

    #endregion Entry Points

    #region Interfaces

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IHandleExceptionOrTry : IFluentInterface
    {
      ///<exception cref="ArgumentNullException">Handler is null</exception>
      IHandleExceptionOrTry Handle<T>(Action<T> Handler) where T : Exception;

      void Try(Action Action);

      T Try<T>(Func<T> Func, T Default);

      ///<exception cref="ArgumentNullException">Handler is null</exception>
      IHandleExceptionOrTry Unhandled(Action<Exception> Handler);
    }

    #endregion Interfaces

    #region Ctor

    private FluentExceptionHandler()
    {
    }

    #endregion Ctor

    #region Implementations

    private class FluentExceptionHandlerImpl : IHandleExceptionOrTry
    {
      private readonly Dictionary<Type, Action<Exception>> handlers = new Dictionary<Type, Action<Exception>>();

      private Action<Exception> unhandledHandler;

      public IHandleExceptionOrTry Handle<T>(Action<T> Handler) where T : Exception
      {
        if (Handler == null)
          throw new ArgumentNullException("Handler", "Handler must not be null");

        var exType = typeof(T);

        handlers.Add(exType, new Action<Exception>(ex =>
        {
          Handler((T)ex);
        }));

        return this;
      }

      public void Try(Action Action)
      {
        Execute<Object>(() =>
        {
          Action();
          return null;
        }, null);
      }

      public T Try<T>(Func<T> Func, T Default)
      {
        return Execute(Func, Default);
      }

      public IHandleExceptionOrTry Unhandled(Action<Exception> Handler)
      {
        if (Handler == null)
          throw new ArgumentNullException("Handler", "Handler must not be null");

        this.unhandledHandler = Handler;
        return this;
      }

      private T Execute<T>(Func<T> Func, T Default)
      {
        try
        {
          return Func();
        }
        catch (Exception ex)
        {
          Action<Exception> handler = null;
          if (handlers.TryGetValue(ex.GetType(), out handler))
          {
            handler(ex);
          }
          else
          {
            if (unhandledHandler != null)
            {
              unhandledHandler(ex);
            }
          }

          return Default;
        }
      }
    }

    #endregion Implementations
  }
}