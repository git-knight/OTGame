import { Component, OnInit } from '@angular/core';

class Unit {
  public name: string;
}

@Component({
  selector: 'app-unit-editor',
  templateUrl: './unit-editor.component.html',
  styleUrls: ['./unit-editor.component.css']
})
export class UnitEditorComponent implements OnInit {

  public units: Unit[]

  constructor() { }

  ngOnInit() {
  }

}
