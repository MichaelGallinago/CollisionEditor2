﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CollisionEditor2.ViewUtilities;

public class TextboxValidator : INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>?> propertyErrors = new();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    public bool HasErrors => propertyErrors.Any();

    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName is null)
        {
            return new Dictionary<string, List<string>?>();
        }

        List<string>? errors = propertyErrors.GetValueOrDefault(propertyName, null);
        return errors ?? Enumerable.Empty<string>();
    }

    public void AddError(string propertyName, string errorMessage)
    {
        if (!propertyErrors.ContainsKey(propertyName))
        {
            propertyErrors.Add(propertyName, new List<string>());
        }

        propertyErrors[propertyName]?.Add(errorMessage);
        OnErrorsChanged(propertyName);
    }
    public void ClearErrors(string propertyName)
    {
        if (propertyErrors.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
        }
    }
    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
}
