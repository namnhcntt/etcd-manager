import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { AppConfig } from '../api/appconfig';

@Injectable()
export class ConfigService {

    config: AppConfig = {
        // theme: 'mdc-dark-indigo',
        theme: 'lara-light-blue',
        dark: true,
        inputStyle: 'outlined',
        ripple: true
    };

    private configUpdate = new Subject<AppConfig>();

    configUpdate$ = this.configUpdate.asObservable();

    updateConfig(config: AppConfig) {
        this.config = config;
        this.configUpdate.next(config);
    }

    getConfig() {
        return this.config;
    }
}
