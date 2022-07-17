import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { EventConstant } from '../constants/event.constant';
import { ComCtxService } from './com-ctx.service';

@Injectable({
    providedIn: 'root'
})
export class AppCtxService {
    private rootCtx?: ComCtxService;
    private childsCtx: ComCtxService[] = [];
    public rootCtxLoaded = new Subject<any>();

    createCtx(ctx: ComCtxService, comId: string) {
        if (comId === 'C000') {
            return null;
        }

        const ctxOp = this.childsCtx.find(x => x.comId === comId);
        if (!ctxOp) {
            if (comId !== 'root') {
                ctx.root = this.rootCtx;
            }

            ctx.comId = comId;
            this.childsCtx.push(ctx);
            return ctx;
        }

        return ctxOp;
    }

    createRootCtx(ctx: ComCtxService) {
        const key = `root`;
        this.rootCtx = ctx;
        this.rootCtx.comId = key;
        this.createCtx(ctx, 'root');
        this.rootCtxLoaded.next(true);

        this.rootCtx.data.fragments = [];
        this.rootCtx.data.formQueue = [];
        this.rootCtx.data.listQueue = [];
        this.rootCtx.data.flags = {};
        this.rootCtx.subscribe(EventConstant.DESTROY_COMPONENT, (componentCtx: ComCtxService) => {
            this.childsCtx.splice(this.childsCtx.indexOf(componentCtx), 1);
        });
    }

    getRootCtx() {
        return this.rootCtx;
    }
}
