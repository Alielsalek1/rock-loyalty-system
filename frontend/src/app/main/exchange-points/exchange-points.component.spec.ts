import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExchangePointsComponent } from './exchange-points.component';

describe('ExchangePointsComponent', () => {
  let component: ExchangePointsComponent;
  let fixture: ComponentFixture<ExchangePointsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ExchangePointsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ExchangePointsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
