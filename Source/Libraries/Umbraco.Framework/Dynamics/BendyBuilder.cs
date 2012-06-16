using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Umbraco.Framework.Dynamics
{
    public class BendyBuilder
    {
        private readonly HashSet<DynamicMethod> _methods = new HashSet<DynamicMethod>();

        public IEnumerable<DynamicMethod> Methods { get { return _methods; } }

        public DynamicMethod CreateMethod<T>(string name, Expression<Func<BendyObject, T>> body)
        {
            var signature = new Signature(name, typeof(T), Param.Create<BendyObject>("bendyObject"));
            var method = new DynamicMethod(signature, body);
            _methods.Add(method);
            return method;
        }

        public DynamicMethod CreateMethod(string name, Expression<Action<BendyObject>> body)
        {
            var signature = new Signature(name, Param.Create<BendyObject>("bendyObject"));
            var method = new DynamicMethod(signature, body);
            _methods.Add(method);
            return method;
        }

        public DynamicMethod CreateMethod<T>(string name, Expression<Action<BendyObject, T>> body, Parameter<T> parameter)
        {
            var signature = new Signature(name, Param.Create<BendyObject>("bendyObject"), parameter);
            var method = new DynamicMethod(signature, body);
            _methods.Add(method);
            return method;
        }

        public DynamicMethod CreateMethod<TOut, TIn1>(string name, Expression<Action<BendyObject, TIn1>> body, string parameter1Name)
        {
            var signature = new Signature(name, typeof(TOut), Param.Create<BendyObject>("bendyObject"), Param.Create<TIn1>(parameter1Name));
            var method = new DynamicMethod(signature, body);
            _methods.Add(method);
            return method;
        }

        public DynamicMethod AutoCreateMethod<TOut, TIn1>(string name, Expression<Func<BendyObject, TIn1, TOut>> body)
        {
            var signature = new Signature(name, typeof(TOut), body.Parameters.Select(parameterExpression => new Parameter(parameterExpression.Name, parameterExpression.Type)).ToArray());
            var method = new DynamicMethod(signature, body);
            _methods.Add(method);
            return method;
        }

        public DynamicMethod AutoCreateMethod<TIn1>(string name, Expression<Action<BendyObject, TIn1>> body)
        {
            var signature = new Signature(name, body.Parameters.Select(parameterExpression => new Parameter(parameterExpression.Name, parameterExpression.Type)).ToArray());
            var method = new DynamicMethod(signature, body);
            _methods.Add(method);
            return method;
        }

        public DynamicMethod AutoCreateMethod<TOut>(string name, Expression<Func<BendyObject, TOut>> body)
        {
            var signature = new Signature(name, body.Parameters.Select(parameterExpression => new Parameter(parameterExpression.Name, parameterExpression.Type)).ToArray());
            var method = new DynamicMethod(signature, body);
            _methods.Add(method);
            return method;
        }

        public BendyObject ToBendy()
        {
            var toReturn = new BendyObject();
            foreach (var dynamicMethod in Methods)
            {
                toReturn.AddMethod(dynamicMethod);
            }
            return toReturn;
        }
    }

    public class DynamicMethod
    {
        public DynamicMethod(Signature signature, Expression body)
        {
            Signature = signature;
            Body = body;
        }

        public Expression Body { get; protected set; }
        public Signature Signature { get; protected set; }
    }

    public class Signature
    {
        private string _name;
        private IEnumerable<Parameter> _parameters;

        public Signature(string name, Type returnType, params Parameter[] parameters)
            : this(name, parameters)
        {
            _name = name;
            _parameters = new HashSet<Parameter>(parameters);
            ReturnType = returnType;
            ReturnsValue = true;
        }

        public Signature(string name, params Parameter[] parameters)
        {
            _name = name;
            _parameters = new HashSet<Parameter>(parameters);
            Parameters = parameters;
            ReturnsValue = false;
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public IEnumerable<Parameter> Parameters
        {
            get { return _parameters; }
            protected set { _parameters = new HashSet<Parameter>(value); }
        }

        public bool ReturnsValue { get; protected set; }
        public Type ReturnType { get; protected set; }

        public override string ToString()
        {
            var paramBuilder = new StringBuilder();
            foreach (var parameter in Parameters)
            {
                paramBuilder.AppendFormat("{0} {1}, ", parameter.Type.Name, parameter.Name);
            }
            return "{0} {1}({2})".InvariantFormat(
                ReturnsValue ? ReturnType.Name : "void", Name, paramBuilder.ToString().Trim(", "));
        }
    }

    public static class Param
    {
        public static Parameter<T> Create<T>(string name)
        {
            return new Parameter<T>(name);
        }
    }

    public class Parameter<T> : Parameter
    {
        public Parameter(string name) : base(name, typeof(T))
        {
        }
    }

    public class Parameter
    {
        public Parameter(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; protected set; }
        public Type Type { get; protected set; }
    }
}
