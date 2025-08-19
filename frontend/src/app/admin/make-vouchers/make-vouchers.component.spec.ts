import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MakeVouchersComponent } from './make-vouchers.component';

describe('MakeVouchersComponent', () => {
  let component: MakeVouchersComponent;
  let fixture: ComponentFixture<MakeVouchersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MakeVouchersComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MakeVouchersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
