/**
 * Autogenerated by Thrift Compiler (0.9.2)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Thrift;
using Thrift.Collections;
using System.Runtime.Serialization;
using Thrift.Protocol;
using Thrift.Transport;

namespace ThriftSharp.Benchmarking.Models.Thrift
{

  
  public partial class BoolContainer : TBase
  {

    public bool Value { get; set; }

    public BoolContainer() {
    }

    public BoolContainer(bool value) : this() {
      this.Value = value;
    }

    public void Read (TProtocol iprot)
    {
      bool isset_value = false;
      TField field;
      iprot.ReadStructBegin();
      while (true)
      {
        field = iprot.ReadFieldBegin();
        if (field.Type == TType.Stop) { 
          break;
        }
        switch (field.ID)
        {
          case 1:
            if (field.Type == TType.Bool) {
              Value = iprot.ReadBool();
              isset_value = true;
            } else { 
              TProtocolUtil.Skip(iprot, field.Type);
            }
            break;
          default: 
            TProtocolUtil.Skip(iprot, field.Type);
            break;
        }
        iprot.ReadFieldEnd();
      }
      iprot.ReadStructEnd();
      if (!isset_value)
        throw new TProtocolException(TProtocolException.INVALID_DATA);
    }

    public void Write(TProtocol oprot) {
      TStruct struc = new TStruct("BoolContainer");
      oprot.WriteStructBegin(struc);
      TField field = new TField();
      field.Name = "value";
      field.Type = TType.Bool;
      field.ID = 1;
      oprot.WriteFieldBegin(field);
      oprot.WriteBool(Value);
      oprot.WriteFieldEnd();
      oprot.WriteFieldStop();
      oprot.WriteStructEnd();
    }

    public override string ToString() {
      StringBuilder __sb = new StringBuilder("BoolContainer(");
      __sb.Append(", Value: ");
      __sb.Append(Value);
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}
