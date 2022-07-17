/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { KeyListComponent } from './key-list.component';

describe('KeyListComponent', () => {
  let component: KeyListComponent;
  let fixture: ComponentFixture<KeyListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ KeyListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(KeyListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
