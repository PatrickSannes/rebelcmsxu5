<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="html" indent="yes"/>

    <xsl:template match="Vertex">
      <h1>
          Hello        
      </h1>
      <xsl:for-each select="Vertex">
        <h2>
          <xsl:value-of select="@Name"/>
        </h2>
        <ul>
          <xsl:for-each select="Property">
            <li>
              <xsl:value-of select="@Name"/>:
              <xsl:value-of select="@Value"/>
            </li>
          </xsl:for-each>
        </ul>

        Foos:
        <xsl:for-each select="Foo">
          <xsl:value-of select="@Name"/>
          Bars:
          <xsl:for-each select="Bar">
            <xsl:value-of select="@Name" />
          </xsl:for-each>
        </xsl:for-each>
      </xsl:for-each>
    </xsl:template>
</xsl:stylesheet>
