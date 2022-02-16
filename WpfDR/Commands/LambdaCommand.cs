using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfDR.Commands.Base;

namespace WpfDR.Commands
{
    public class LambdaCommand : Command
    {
        private readonly Action<object> _execute;

        private readonly Func<object, bool> _canExecute1;

        public LambdaCommand(Action<object> Execute, Func<object, bool> CanExecute = null)
        {
            _execute = Execute;
            _canExecute1 = CanExecute;
        }

        public override void Execute(object parameter) => _execute(parameter);

        public override bool CanExecute(object parameter) => _canExecute1?.Invoke(parameter) ?? true;


    }
}
