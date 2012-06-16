<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
  xmlns:test="urn:test"
>
    <xsl:output method="text" indent="yes"/>

  <xsl:template match="Vertex">
    Vertex <xsl:value-of select="@Name" />


    <xsl:for-each select="test:Load(., 'Foo')">
      Loaded: <xsl:value-of select="@Name" />
      Properties:
      <xsl:for-each select="Property">
        <xsl:value-of select="@Name"/>: <xsl:value-of select="@Value"/>&#160;
      </xsl:for-each>
    </xsl:for-each>


    Properties:
    <xsl:for-each select="Property">
        <xsl:value-of select="@Name"/>: <xsl:value-of select="@Value"/>&#160;      
      </xsl:for-each>
    <xsl:for-each select="Vertex">
      Children:
      <xsl:apply-templates select="."></xsl:apply-templates>
    </xsl:for-each>   
  </xsl:template>
  
</xsl:stylesheet>
