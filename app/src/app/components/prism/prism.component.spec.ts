/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { PrismComponent } from './prism.component';

describe('PrismComponent', () => {
  let component: PrismComponent;
  let fixture: ComponentFixture<PrismComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PrismComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PrismComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
