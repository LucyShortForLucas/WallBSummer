#nullable enable
using NUnit.Framework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class TooltipHandler : MonoBehaviour
{
    // ---- Handle class
    public class Handle
    {
        // ---- Data
        private readonly TooltipHandler _handler;

        private LinkedListNode<TooltipData>? _backedNode;

        // ---- Ctor + Dtor + Free
        public Handle(TooltipHandler handler, TooltipHandler.TooltipData data)
        {
            _handler = handler;

            NewTooltip(data);
        }

        public Handle(TooltipHandler handler)
        {
            _handler = handler;
        }

        ~Handle()
        {
            if (_backedNode != null)
                RemoveTooltip();
        }

        // API
        public bool Empty => _backedNode == null;
        public void NewTooltip(TooltipHandler.TooltipData data)
        {
            if (_backedNode != null)
                RemoveTooltip();

            _handler._tooltips.AddFirst(data);

            _backedNode = _handler._tooltips.First;

            _handler.ShowTooltip(data);
        }

        public void RemoveTooltip()
        {
            if (_backedNode == null)
                return;

            _handler._tooltips.Remove(_backedNode);

            _backedNode = null;

            if (_handler._tooltips.Count == 0)
                _handler.HideTooltip();
            else
                _handler.ShowTooltip(_handler._tooltips.First.Value);
        }
    }

    // ---- Config

    [Header("Config")]
    [SerializeField] private int _toolTipSize;

    [Header("Scene Agnostic")]
    [SerializeReference] private TextMeshProUGUI? _tmPro;


    // ---- Data

    public struct TooltipData
    { 
        public bool Freed; public string Text; public Color TextColor; public Color BackgroundColor;
        public TooltipData(string text, Color textColor, Color backGroundColor) 
            => (Freed, Text, TextColor, BackgroundColor) = (false, text, textColor, backGroundColor);
    }

    private LinkedList<TooltipData> _tooltips = new();

    // Public API

    public Handle NewToolTip(string text,  Color textColor, Color backgroundColor)
    {
        return new Handle(this, new TooltipData(text, textColor, backgroundColor));
    }

    public Handle NewToolTip(TooltipData data)
    {
        return new Handle(this, data);
    }

    public Handle NewHandle()
    {
        return new Handle(this);
    }

    // ---- private control methods
    private void ShowTooltip(TooltipData data)
    {
        if (_tmPro == null)
            return;

        _tmPro.text = data.Text;
        _tmPro.color = data.TextColor;
        _tmPro.fontSize = _toolTipSize;

        _tmPro.gameObject.SetActive(true);
    }

    private void HideTooltip()
    {
        if (_tmPro == null)
            return;

        _tmPro.gameObject.SetActive(false);
    }

    // ---- Unity methods

    private void Update()
    {
        if (_tmPro == null)
            return;

        _tmPro.transform.position = Mouse.current.position.ReadValue();
    }
}