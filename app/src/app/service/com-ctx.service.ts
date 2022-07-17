import { Injectable } from '@angular/core';
import { ReplaySubject, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { EventConstant } from '../constants/event.constant';

@Injectable()
export class ComCtxService {
    /** Mã Component */
    comId?: string;
    /** Context Cha */
    parent?: ComCtxService;
    /** Context gốc */
    root?: ComCtxService;
    /** Dữ liệu trong context */
    data = <any>{};

    _unSubscribeAll = new Subject<any>();
    events: { [name: string]: Subject<any> } = {};
    replayEvents: { [name: string]: ReplaySubject<any> } = {};

    private createEvent(eventName: string, event?: Subject<any>): Subject<any> {
        const key = `${this.comId}.${eventName}`;
        if (!this.events[key] || !this.events[key].observers) {
            if (!event) {
                event = new Subject<any>();
            }
            this.events[key] = event;
        }

        return this.events[key];
    }

    private createReplayEvent(eventName: string, event?: ReplaySubject<any>): ReplaySubject<any> {
        const key = `${this.comId}.${eventName}`;
        if (!this.replayEvents[key] || !this.replayEvents[key].observers) {
            if (!event) {
                event = new ReplaySubject<any>();
            }
            this.replayEvents[key] = event;
        }

        return this.replayEvents[key];
    }

    dispatchEvent(eventName: string, data?: any, cancelBubble: boolean = true) {
        const event = this.createEvent(eventName);
        event.next(data);
        if (!cancelBubble) {
            if (this.parent && this.parent !== this.root) {
                this.parent.dispatchEvent(eventName, data);
            }
            else if (!this.parent && this.root && this !== this.root) {
                this.root.dispatchEvent(eventName, data);
            }
        }
    }

    dispatchReplayEvent(eventName: string, data?: any, cancelBubble: boolean = true) {
        const event = this.createReplayEvent(eventName);
        event.next(data);
        if (!cancelBubble) {
            if (this.parent && this.parent !== this.root) {
                this.parent.dispatchReplayEvent(eventName, data);
            }
            else if (!this.parent && this.root && this !== this.root) {
                this.root.dispatchReplayEvent(eventName, data);
            }
        }
    }

    completeEvent(name: string) {
        const event = this.createEvent(name);
        event.next(null);
        event.complete();
    }

    completeReplayEvent(name: string) {
        const event = this.createReplayEvent(name);
        event.next(null);
        event.complete();
    }

    private completeEventInternal(name: string) {
        const key = `${this.comId}.${name}`;
        if (this.events[key]) {
            this.events[key].complete();
            delete this.events[key];
        }
    }

    private completeReplayEventInternal(name: string) {
        const key = `${this.comId}.${name}`;
        if (this.replayEvents[key]) {
            this.replayEvents[key].complete();
            delete this.replayEvents[key];
        }
    }

    private getEvent(name: string) {
        const key = `${this.comId}.${name}`;

        if (!this.events[key] || !this.events[key].observers) {
            this.events[key] = new Subject<any>();
        }

        return this.events[key];
    }

    private removeEvent(name: string) {
        const key = `${this.comId}.${name}`;

        if (this.events[key]) {
            this.events[key].unsubscribe();
        }
    }

    private getReplayEvent(name: string) {
        const key = `${this.comId}.${name}`;

        if (!this.replayEvents[key] || !this.replayEvents[key].observers) {
            this.replayEvents[key] = new ReplaySubject<any>();
        }

        return this.replayEvents[key];
    }

    private removeReplayEvent(name: string) {
        const key = `${this.comId}.${name}`;

        if (this.replayEvents[key]) {
            this.replayEvents[key].unsubscribe();
        }
    }

    replaySubscribe(name: string, callBack: any) {
        return this.getReplayEvent(name)
            .pipe(takeUntil(this._unSubscribeAll))
            .subscribe((rs) => {
                this.commonCallBack(callBack, rs);
            });
    }

    replaySubscribeOnce(name: string, callBack: any) {
        const sub = this.getReplayEvent(name)
            .pipe(takeUntil(this._unSubscribeAll))
            .subscribe((rs) => {
                try {
                    callBack(rs);
                }
                catch { }
                if (sub) {
                    sub.unsubscribe();
                }
            });
    }

    subscribe(name: string, callBack: any) {
        return this.getEvent(name)
            .pipe(takeUntil(this._unSubscribeAll))
            .subscribe((rs) => {
                this.commonCallBack(callBack, rs);
            });
    }

    private commonCallBack(callBack: any, rs: any) {
        try {
            callBack(rs);
        }
        catch { }
    }

    subscribeOnce(name: string, callBack: any) {
        const sub = this.getEvent(name)
            .pipe(takeUntil(this._unSubscribeAll))
            .subscribe((rs) => {
                this.commonCallBack(callBack, rs);
                if (sub) {
                    sub.unsubscribe();
                }
            });
    }

    unSubscribe(name: string) {
        if (this.events[`${this.comId}.${name}`]) {
            this.events[`${this.comId}.${name}`].unsubscribe();
            delete this.events[`${this.comId}.${name}`];
        }
    }

    unSubscribleReplay(name: string) {
        if (this.replayEvents[`${this.comId}.${name}`]) {
            this.replayEvents[`${this.comId}.${name}`].unsubscribe();
            delete this.replayEvents[`${this.comId}.${name}`];
        }
    }

    destroyContext() {
        for (const key in this.events) {
            this.completeEventInternal(key);
        }

        for (const key in this.replayEvents) {
            this.completeReplayEventInternal(key);
        }

        this._unSubscribeAll.next(null);
        this._unSubscribeAll.complete();

        this.data = {};
        if (this.root) {
            this.root.dispatchEvent(EventConstant.DESTROY_COMPONENT, this);
        }
    }
}
