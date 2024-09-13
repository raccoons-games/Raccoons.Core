using Raccoons.UI.Guids;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Raccoons.UI.Screens
{
    public interface IScreen
    {
        event EventHandler<IScreen> OnOpened;
        event EventHandler<IScreen> OnClosed;
        string Key { get; }
        public IUIGuid Guid { get; }

        Task Open(CancellationToken cancellationToken = default);
        Task Close(CancellationToken cancellationToken = default);
        bool IsOpen();
    }
}