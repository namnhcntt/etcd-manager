import { Component, OnInit } from '@angular/core';
import { AppEventService } from 'src/app/service/app-event.service';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

    constructor(
        private _appEventService: AppEventService
    ) { }

    ngOnInit() {

    }

}
