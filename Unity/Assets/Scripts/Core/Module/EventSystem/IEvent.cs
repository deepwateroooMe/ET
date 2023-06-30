﻿using System;
namespace ET {
    public interface IEvent {
        Type Type { get; }
    }
    
    public abstract class AEvent<A>: IEvent where A: struct {
        public Type Type {
            get {
                return typeof (A);
            }
        }
        protected abstract ETTask Run(Scene scene, A a);
        public async ETTask Handle(Scene scene, A a) { // 试着去找一个，什么地方会调用这个方法吗？
            try {
                await Run(scene, a);
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }
    }
}