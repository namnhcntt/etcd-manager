import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-connection-manager',
    templateUrl: './connection-manager.component.html',
    styleUrls: ['./connection-manager.component.scss']
})
export class ConnectionManagerComponent implements OnInit {
    loading = false;
    connections = [];

    constructor() { }

    ngOnInit() {
    }

}
